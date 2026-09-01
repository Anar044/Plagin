using Microsoft.Extensions.DependencyInjection;
using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Data.Common;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Persistence.Repositories.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using Resto.Front.Api.HorecaControlPlugin.Sql;
using Resto.Front.Api.HorecaControlPlugin.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Resto.Front.Api.HorecaControlPlugin.Notifiers;

internal sealed class DeliveryOrderChangeNotifier : IDisposable
{
    private readonly CompositeDisposable subscriptions = new CompositeDisposable();

    private readonly IEventPublisher _eventPublisher;
    private readonly IRepository _repository;
    private readonly HorecaSqlite _context; // Для обратной совместимости

    public DeliveryOrderChangeNotifier(IServiceProvider serviceProvider)
    {
        _eventPublisher = serviceProvider.GetService<IEventPublisher>();
        _repository = serviceProvider.GetService<IRepository>();
        _context = serviceProvider.GetRequiredService<HorecaSqlite>();
        var deliveryOrders = PluginContext.Operations.GetDeliveryOrders(true);
        // Теперь удаленные заказы выставляются в api
        var deletedAndStornedOrderIds = deliveryOrders
            .Where(o => o.Status == OrderStatus.Deleted)
            .Select(o => o.Id)
            .ToConcurrentHashSet();


        subscriptions.Add(PluginContext.Notifications.DeliveryOrderChanged
            .Where(s =>
                s.EventType == EntityEventType.Updated
                // && s.Entity.Status == OrderStatus.New
                && s.Entity.StornedOrderId == null)
            .Where(x => LastChangedTerminalInHostGroup(x.Entity))
            .Select(x => x.Entity)
            .Subscribe(OnDeliveryOrderCreating, OnError));

        //OK
        var closedOrderIds = deliveryOrders
            .Where(o => o.Status == OrderStatus.Closed)
            .Select(o => o.Id)
            .ToList();


        subscriptions.Add(
            PluginContext.Notifications.BeforeDeleteOrder
                .Subscribe(x =>
                {
                    try
                    {
                        deletedAndStornedOrderIds.Add(x.order.Id);
                        OnDeliveryOrderDeleting(x.order);
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
                }));


        // subscriptions.Add(
        //     PluginContext.Notifications.DeliveryOrderChanged
        //         .Select(e => e.Entity)
        //         .Where(o =>
        //         o.Status == OrderStatus.Closed && 
        //         !closedOrderIds.Contains(o.Id) && 
        //          !deletedAndStornedOrderIds.Contains(o.Id))
        //         .Do(o => closedOrderIds.Add(o.Id))
        //         .Where(LastChangedTerminalInHostGroup)
        //         .Subscribe(OnDeliveryOrderClosed));

        // subscriptions.Add(
        //     PluginContext.Notifications.DeliveryOrderChanged
        //         .Select(e => e.Entity)
        //         .Where(o => o.Status == OrderStatus.Deleted
        //                     && !deletedAndStornedOrderIds.Contains(o.Id))
        //         .Do(o => deletedAndStornedOrderIds.Add(o.Id))
        //         .Where(LastChangedTerminalInHostGroup)
        //         .Subscribe(OnDeliveryOrderStorned));
    }

    private void OnError(Exception ex)
    {
        PluginContext.Log.Error($"Error in DeliveryOrderChangeNotifier subscription: {ex.Message} ", ex);
    }

    private void OrdersWithTooLongBillTimeExists(List<IDeliveryOrder> billOrders)
    {
        PluginContext.Log.Debug($"DeliveryOrdersWithTooLongBillTimeExists :: started.");
        var dt = DateTime.Now;


        billOrders.ForEach(o =>
        {
            var longBill = (dt - o.BillTime.Value).TotalMinutes;
            PublishEvent(new PluginToServerEvent
            {
                PluginEventType = EnumPluginEventType.OrderInTheStatusOfTheBillForALongTime,
                Data = new PluginToServerEventOrder
                {
                    Tables = o.Tables.GetTablesAsString(),
                    OrderNum = o.Number,
                    Floor = o.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                    Waiter = o.DeliveryOperator?.Name ?? string.Empty,
                    Cashier = o.Cashier?.Name ?? string.Empty,
                    OpenTime = o.OpenTime,
                    CloseTime = o.CloseTime,
                    BillTime = o.BillTime,
                    Revenue = o.ResultSum,
                    IsBanquet = o.IsBanquetOrder,
                    OrderInBillTooLong = (int)longBill,
                }
            });
        });

        PluginContext.Log.Debug($"DeliveryOrdersWithTooLongBillTimeExists :: finished.");
    }


    private bool LastChangedTerminalInHostGroup(IDeliveryOrder order)
    {
        var terminals = PluginContext.Operations.GetTerminalsGroupTerminals(PluginHelpers.GroupName);
        var terminal = PluginContext.Operations.GetTerminalById(order.LastChangedTerminalId);
        return terminals.Contains(terminal);
    }

    private void OnDeliveryOrderClosed(IDeliveryOrder order)
    {
        var hasChanges = false;
        try
        {
            PluginContext.Log.Info($"OnDeliveryOrderClosed :: Number {order.Number}");
            PublishEvent(new PluginToServerEvent
            {
                PluginEventType = EnumPluginEventType.ClosingOrder,
                Data = new PluginToServerEventOrder
                {
                    Tables = order.Tables.GetTablesAsString(),
                    OrderNum = order.Number,
                    Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                    Waiter = order.DeliveryOperator?.Name ?? string.Empty,
                    Cashier = order.Cashier?.Name ?? string.Empty,
                    OpenTime = order.OpenTime,
                    CloseTime = order.CloseTime,
                    BillTime = order.BillTime,
                    Revenue = order.ResultSum,
                    IsDelivery = true,
                }
            });
            DateTime? closeTime = order.CloseTime.HasValue ? order.CloseTime.Value : null;
            DateTime? normalCloseTime = order.BillTime.HasValue
                ? order.BillTime.Value.AddMinutes(Properties.Settings.Default.PeriodBetweenGuestBillAndCheckInMin)
                : null;
            if (closeTime.HasValue && normalCloseTime.HasValue)
            {
                var totalMinutes = (closeTime.Value - normalCloseTime.Value).TotalMinutes;

                if (closeTime > normalCloseTime)
                    PublishEvent(new PluginToServerEvent
                    {
                        PluginEventType = EnumPluginEventType.ThePeriodBetweenGuestBillAndCheckIsTooLong,
                        Data = new PluginToServerEventOrder
                        {
                            Tables = order.Tables.GetTablesAsString(),
                            OrderNum = order.Number,
                            Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                            Waiter = order.Waiter?.Name ?? string.Empty,
                            Cashier = order.Cashier?.Name ?? string.Empty,
                            OpenTime = order.OpenTime,
                            CloseTime = order.CloseTime,
                            BillTime = order.BillTime,
                            Revenue = order.ResultSum,
                            IsBanquet = order.IsBanquetOrder,
                            TotalMinutesBetweenBillAndClose = (int)totalMinutes,
                        }
                    });
            }
        }
        catch (Exception ex)
        {
            PluginContext.Log.Error($"OnDeliveryOrderClosed :: {ex.Message}", ex);
        }
    }

    private void OnDeliveryOrderStorned(IDeliveryOrder order)
    {
        try
        {
            PluginContext.Log.Info($"OnDeliveryOrderStorned :: Number {order.Number}");
            if (order.StornedOrderId != null)
                return;
            PublishEvent(new PluginToServerEvent
            {
                PluginEventType = (order.ResultSum == 0)
                    ? EnumPluginEventType.DeleteAnEmptyOrder
                    : EnumPluginEventType.VoidReceipt,
                Data = new PluginToServerEventOrder
                {
                    Tables = order.Tables.GetTablesAsString(),
                    OrderNum = order.Number,
                    Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                    Waiter = order.DeliveryOperator?.Name ?? string.Empty,
                    Cashier = order.Cashier?.Name ?? string.Empty,
                    CloseTime = DateTime.Now,
                    Revenue = order.ResultSum > 0 ? order.ResultSum : null,
                    IsDelivery = true,
                }
            });

            if (_repository != null)
                _repository.AddHighRiskOperation(order.Waiter, "orderVoided");
            else
                _context.AddHighRiskOperation(order.Waiter, "orderVoided");
        }
        catch (Exception ex)
        {
            PluginContext.Log.Error($"OnDeliveryOrderStorned :: {ex.Message}", ex);
        }
    }

    private void OnDeliveryOrderDeleting(IOrder o)
    {
        if (o is IDeliveryOrder order)
        {
            try
            {
                if (!LastChangedTerminalInHostGroup(order))
                    return;

                PluginContext.Log.Info($"OnDeliveryOrderDeleting :: Number {order.Number}");
                if (order.StornedOrderId != null)
                    return;
                PublishEvent(new PluginToServerEvent
                {
                    PluginEventType = (order.ResultSum == 0)
                        ? EnumPluginEventType.DeleteAnEmptyOrder
                        : EnumPluginEventType.DeletingAnOrder,
                    Data = new PluginToServerEventOrder
                    {
                        Tables = order.Tables.GetTablesAsString(),
                        OrderNum = order.Number,
                        Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                        Waiter = order.DeliveryOperator?.Name ?? string.Empty,
                        Cashier = order.Cashier?.Name ?? string.Empty,
                        CloseTime = DateTime.Now,
                        Revenue = order.ResultSum > 0 ? order.ResultSum : null,
                        IsDelivery = true,
                    }
                });
                if (_repository != null)
                {
                    _repository.SaveOrder(order, true);
                    _repository.AddHighRiskOperation(order.Waiter, "orderDeleted");
                }
                else
                {
                    _context.SaveOrder(order, true);
                    _context.AddHighRiskOperation(order.Waiter, "orderDeleted");
                }
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error($"OnDeliveryOrderDeleting :: {ex.Message}", ex);
            }
        }
    }

    private void OnDeliveryOrderCreating(IDeliveryOrder order)
    {
        var hasChanges = false;
        try
        {
            var deliveryStatus = order.DeliveryStatus switch
            {
                DeliveryStatus.Unconfirmed => EnumDeliveryOrderStatusDto.Unconfirmed,
                DeliveryStatus.New => EnumDeliveryOrderStatusDto.New,
                DeliveryStatus.Waiting => EnumDeliveryOrderStatusDto.Waiting,
                DeliveryStatus.OnWay => EnumDeliveryOrderStatusDto.OnWay,
                DeliveryStatus.Delivered => EnumDeliveryOrderStatusDto.Delivered,
                DeliveryStatus.Closed => EnumDeliveryOrderStatusDto.Closed,
                DeliveryStatus.Cancelled => EnumDeliveryOrderStatusDto.Cancelled,
                _ => EnumDeliveryOrderStatusDto.Unconfirmed
            };


            LocalOrder oldOrder = null;
            if (_repository != null)
            {
                oldOrder = _repository.GetOrder(order.Id);
            }
            else
            {
                oldOrder = _context.LoadOrder(order);
            }
            var isNewOrder = oldOrder is null;
            if (isNewOrder)
            {
                PluginContext.Log.Info($"OnDeliveryOrderCreating :: Number {order.Number}");

                PublishEvent(new PluginToServerEvent
                {
                    PluginEventType = EnumPluginEventType.NewOrder,
                    Data = new PluginToServerEventOrder
                    {
                        Tables = order.Tables.GetTablesAsString(),
                        OrderNum = order.Number,
                        Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                        Waiter = order.DeliveryOperator?.Name ?? string.Empty,
                        Cashier = order.Cashier?.Name ?? string.Empty,
                        OpenTime = order.CreateTime,
                        DeliveryStatus = deliveryStatus,
                        IsDelivery = true,
                    }
                });
                if (_repository != null)
                {
                    _repository.AddHighRiskOperation(order.Waiter, "orderCreated");
                    oldOrder = _repository.SaveOrder(order);
                }
                else
                {
                    _context.AddHighRiskOperation(order.Waiter, "orderCreated");
                    oldOrder = _context.SaveOrder(order);
                }
            }

            // Проверить смену официанта
            if (order.DeliveryOperator != null)
            {
                if (oldOrder.WaiterId != order.DeliveryOperator.Id)
                {
                    PublishEvent(new PluginToServerEvent
                    {
                        PluginEventType = EnumPluginEventType.OrdersWaiterHasChanged,
                        Data = new PluginToServerEventWaiterChanged
                        {
                            Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                            OldWaiterName = oldOrder.WaiterName,
                            NewWaiterName = order.DeliveryOperator?.Name ?? string.Empty,
                            OrderNum = order.Number,
                            Revenue = order.ResultSum,
                            Tables = order.Tables.GetTablesAsString(),
                        }
                    });
                    if (_repository != null)
                        _repository.AddHighRiskOperation(order.DeliveryOperator, "waiterChanged");
                    else
                        _context.AddHighRiskOperation(order.DeliveryOperator, "waiterChanged");
                    hasChanges = true;
                }
            }

            // Проверить смену столов
            var newTables = LocalOrder.GetTables(order);
            var newTablesTracker =
                new ChangesTracker<KeyValueLocalOrderClass, KeyValueLocalOrderClass>(oldOrder.Tables, newTables,
                    AreEquals);
            if (newTablesTracker.AddedItems.Any())
                foreach (var item in newTablesTracker.AddedItems)
                {
                    PublishEvent(new PluginToServerEvent
                    {
                        PluginEventType = EnumPluginEventType.OrderTableHasBeenChanged,
                        Data = new PluginToServerEventTableChanged
                        {
                            Waiter = order.Waiter?.Name ?? string.Empty,
                            OldFloor = oldOrder.Floor,
                            NewFloor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                            OrderNum = order.Number,
                            Revenue = order.ResultSum,
                            //TODO: Может багануть
                            OldTables = string.Join(" ,",
                                oldOrder.Tables.Select(x => $"{(int)x.Value.GetValueOrDefault(0)}")?.ToList() ??
                                new List<string>()),
                            NewTables = order.Tables.GetTablesAsString(),
                        }
                    });
                    if (_repository != null)
                        _repository.AddHighRiskOperation(order.Waiter, "tableChanged");
                    else
                        _context.AddHighRiskOperation(order.Waiter, "tableChanged");
                    hasChanges = true;
                }


            // Проверить скидки надбавки
            var newDiscount = LocalOrder.GetDiscountsSurchargesLists(order);

            // Для нового заказа SaveOrder уже сохранил текущие скидки/надбавки — иначе adding* не сработает.
            var discountBaseline = isNewOrder
                ? new List<KeyValueLocalOrderClass>()
                : (oldOrder.Discounts ?? new List<KeyValueLocalOrderClass>());
            var surchargeBaseline = isNewOrder
                ? new List<KeyValueLocalOrderClass>()
                : (oldOrder.Surcharges ?? new List<KeyValueLocalOrderClass>());

            var newDiscountTracker =
                new ChangesTracker<KeyValueLocalOrderClass, KeyValueLocalOrderClass>(discountBaseline,
                    newDiscount.Discounts, AreEquals);
            var newSurchargeTracker =
                new ChangesTracker<KeyValueLocalOrderClass, KeyValueLocalOrderClass>(surchargeBaseline,
                    newDiscount.Surcharges, AreEquals);
            if (newSurchargeTracker.AddedItems.Any())
            {
                foreach (var item in newSurchargeTracker.AddedItems)
                {
                    PublishEvent(new PluginToServerEvent
                    {
                        PluginEventType = EnumPluginEventType.AddingSurcharge,
                        Data = new PluginToServerEventAddDiscountSurchargeItem
                        {
                            Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                            Name = item.Name,
                            Value = item.Value.GetValueOrDefault(0),
                            ValuePercent = item.ValuePercent.GetValueOrDefault(0),
                            OrderNum = order.Number,
                            Waiter = order.Waiter?.Name ?? string.Empty,
                            Revenue = order.ResultSum,
                            Tables = order.Tables.GetTablesAsString(),
                        }
                    });
                    if (_repository != null)
                        _repository.AddHighRiskOperation(order.Waiter, "addingSurcharge");
                    else
                        _context.AddHighRiskOperation(order.Waiter, "addingSurcharge");
                }

                hasChanges = true;
            }

            if (newSurchargeTracker.RemovedItems.Any())
            {
                foreach (var item in newSurchargeTracker.RemovedItems)
                {
                    PublishEvent(new PluginToServerEvent
                    {
                        PluginEventType = EnumPluginEventType.RemovingSurcharge,
                        Data = new PluginToServerEventAddDiscountSurchargeItem
                        {
                            Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                            Name = item.Name,
                            Value = item.Value.GetValueOrDefault(0),
                            ValuePercent = item.ValuePercent.GetValueOrDefault(0),
                            OrderNum = order.Number,
                            Waiter = order.Waiter?.Name ?? string.Empty,
                            Revenue = order.ResultSum,
                            Tables = order.Tables.GetTablesAsString(),
                        }
                    });
                    if (_repository != null)
                        _repository.AddHighRiskOperation(order.Waiter, "removingSurcharge");
                    else
                        _context.AddHighRiskOperation(order.Waiter, "removingSurcharge");
                }

                hasChanges = true;
            }

            if (newDiscountTracker.AddedItems.Any())
            {
                foreach (var item in newDiscountTracker.AddedItems)
                {
                    PublishEvent(new PluginToServerEvent
                    {
                        PluginEventType = EnumPluginEventType.AddingDiscount,
                        Data = new PluginToServerEventAddDiscountSurchargeItem
                        {
                            Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                            Name = item.Name,
                            Value = item.Value.GetValueOrDefault(0),
                            ValuePercent = item.ValuePercent.GetValueOrDefault(0),
                            OrderNum = order.Number,
                            Waiter = order.Waiter?.Name ?? string.Empty,
                            Revenue = order.ResultSum,
                            Tables = order.Tables.GetTablesAsString(),
                        }
                    });
                    if (_repository != null)
                        _repository.AddHighRiskOperation(order.Waiter, "addingDiscount");
                    else
                        _context.AddHighRiskOperation(order.Waiter, "addingDiscount");
                }

                hasChanges = true;
            }

            if (newDiscountTracker.RemovedItems.Any())
            {
                foreach (var item in newDiscountTracker.RemovedItems)
                {
                    PublishEvent(new PluginToServerEvent
                    {
                        PluginEventType = EnumPluginEventType.RemovingDiscount,
                        Data = new PluginToServerEventAddDiscountSurchargeItem
                        {
                            Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                            Name = item.Name,
                            Value = item.Value.GetValueOrDefault(0),
                            ValuePercent = item.ValuePercent.GetValueOrDefault(0),
                            OrderNum = order.Number,
                            Waiter = order.Waiter?.Name ?? string.Empty,
                            Revenue = order.ResultSum,
                            Tables = order.Tables.GetTablesAsString(),
                        }
                    });
                    if (_repository != null)
                        _repository.AddHighRiskOperation(order.Waiter, "removingDiscount");
                    else
                        _context.AddHighRiskOperation(order.Waiter, "removingDiscount");
                }

                hasChanges = true;
            }

            var canSend = false;
            var enumSendCommand = EnumPluginEventType.NewOrder;
            int minutes = 0;
            if (oldOrder.DeliveryStatus.HasValue)
            {
                if (oldOrder.DeliveryStatus.Value != order.DeliveryStatus)
                {
                    if (oldOrder.DeliveryStatus == DeliveryStatus.Delivered
                        && order.DeliveryStatus == DeliveryStatus.OnWay
                       )
                    {
                        // Перевод в неотправлен
                        canSend = true;
                        enumSendCommand = EnumPluginEventType.DeliveryOrderNotDelivered;
                    }
                    else if (order.DeliveryStatus == DeliveryStatus.Delivered)
                    {
                        // Доставлен
                        canSend = true;
                        enumSendCommand = EnumPluginEventType.DeliveryOrderDelivered;
                        minutes = (int)(order.ActualDeliverTime.GetValueOrDefault(DateTime.Now) -
                                        order.SendTime.GetValueOrDefault(DateTime.Now)).TotalMinutes;
                    }
                    else if (order.DeliveryStatus == DeliveryStatus.OnWay)
                    {
                        // в пути
                        canSend = true;
                        enumSendCommand = EnumPluginEventType.DeliveryOrderOnItsWay;
                    }
                    else if (order.DeliveryStatus == DeliveryStatus.Cancelled)
                    {
                        // отменен
                        canSend = true;
                        enumSendCommand = EnumPluginEventType.DeliveryOrderCancelled;
                    }
                    else if (order.DeliveryStatus == DeliveryStatus.Closed)
                    {
                        canSend = true;
                        enumSendCommand = EnumPluginEventType.ClosingOrder;
                        if (_repository != null)
                            _repository.AddHighRiskOperation(order.DeliveryOperator, "orderClosed");
                        else
                            _context.AddHighRiskOperation(order.DeliveryOperator, "orderClosed");
                    }

                    hasChanges = true;
                }
            }

            if (canSend)
            {
                if (oldOrder.ShiftCount > 0 && enumSendCommand == EnumPluginEventType.ClosingOrder)
                {
                    PublishEvent(new PluginToServerEvent
                    {
                        PluginEventType = EnumPluginEventType.SeveralOrderShifts,
                        Data = new PluginToServerEventOrder
                        {
                            Tables = order.Tables.GetTablesAsString(),
                            OrderNum = order.Number,
                            Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                            Waiter = order.DeliveryOperator?.Name ?? string.Empty,
                            Cashier = order.Cashier?.Name ?? string.Empty,
                            OpenTime = order.CreateTime,
                            CloseTime = order.DeliveryCloseTime,
                            DeliveryStatus = deliveryStatus,
                            IsDelivery = true,
                            Revenue = order.ResultSum,
                            Minutes = minutes,
                            OrderShiftCount = oldOrder.ShiftCount,
                        }
                    });
                }

                PublishEvent(new PluginToServerEvent
                {
                    PluginEventType = enumSendCommand,
                    Data = new PluginToServerEventOrder
                    {
                        Tables = order.Tables.GetTablesAsString(),
                        OrderNum = order.Number,
                        Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                        Waiter = order.DeliveryOperator?.Name ?? string.Empty,
                        Cashier = order.Cashier?.Name ?? string.Empty,
                        OpenTime = order.CreateTime,
                        CloseTime = order.DeliveryCloseTime,
                        DeliveryStatus = deliveryStatus,
                        IsDelivery = true,
                        Revenue = order.ResultSum,
                        Minutes = minutes,
                    }
                });
            }
        }
        catch (Exception e)
        {
        }
        finally
        {
            if (hasChanges)
            {
                if (_repository != null)
                    _repository.SaveOrder(order);
                else
                    _context.SaveOrder(order);
            }
        }
    }

    private static bool AreEquals(KeyValueLocalOrderClass old, KeyValueLocalOrderClass newL, bool update)
    {
        if (old is null && newL is null)
            return true;
        if (old is null || newL is null)
            return false;

        return
            (old.Id == newL.Id);
    }


    private void PublishEvent(PluginToServerEvent evt)
    {
        if (_eventPublisher != null)
        {
            _eventPublisher.PublishEvent(evt);
        }
        else
        {
            PluginContext.Log.Warn("DeliveryOrderChangeNotifier :: IEventPublisher not available, event will not be published.");
        }
    }

    public void Dispose()
    {
        subscriptions?.Dispose();
    }
}