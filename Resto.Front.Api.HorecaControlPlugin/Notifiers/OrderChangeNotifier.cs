using Microsoft.Extensions.DependencyInjection;
using Resto.Front.Api.Data.Common;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Persistence.Repositories.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using Resto.Front.Api.HorecaControlPlugin.Sql;
using Resto.Front.Api.HorecaControlPlugin.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Resto.Front.Api.HorecaControlPlugin.Notifiers
{
    internal sealed class OrderChangeNotifier : IDisposable
    {
        private readonly CompositeDisposable subscriptions = new CompositeDisposable();
        private readonly IRepository _repository;
        private readonly IEventPublisher _eventPublisher;
        private readonly HorecaSqlite _context; // Для обратной совместимости, если IRepository не передан

        public OrderChangeNotifier(IServiceProvider serviceProvider)
        {
            // При старте надо собрать все ,проверить есть ли они в БД и держать их у себя

            // Используем интерфейсы, если они зарегистрированы, иначе используем старые зависимости
            _eventPublisher = serviceProvider.GetService<IEventPublisher>();
            _repository = serviceProvider.GetService<IRepository>();
            _context = serviceProvider.GetRequiredService<HorecaSqlite>();

            var orders = PluginContext.Operations.GetOrders(true, true);
            // Теперь удаленные заказы выставляются в api
            var deletedAndStornedOrderIds = orders
                .Where(o => o.Status == OrderStatus.Deleted)
                .Select(o => o.Id)
                .ToConcurrentHashSet();

            // subscriptions.Add(PluginContext.Notifications.OrderChanged.Subscribe(x=>OrderTest(x)));
            //STP1
            subscriptions.Add(PluginContext.Notifications.OrderChanged
                .Where(s =>
                    s.EventType == EntityEventType.Updated
                    && s.Entity.Status == OrderStatus.New
                )
                .Where(x => LastChangedTerminalInHostGroup(x.Entity))
                // .ObserveOn(TaskPoolScheduler.Default)
                // .Subscribe(x => Task.Run(() => OnOrderCreating(x.Entity)),
                .Subscribe(x => OnOrderCreating(x.Entity),
                    OnError));

            //OK
            var closedOrderIds = orders
                .Where(o => o.Status == OrderStatus.Closed)
                .Select(o => o.Id)
                .ToList();


            subscriptions.Add(
                PluginContext.Notifications.BeforeDeleteOrder
                    .Subscribe(x =>
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    deletedAndStornedOrderIds.Add(x.order.Id);
                                    OnOrderDeleting(x.order);
                                }
                                catch (Exception ex)
                                {
                                    OnError(ex);
                                }
                            });
                        }
                    ));

            subscriptions.Add(
                PluginContext.Notifications.OrderChanged
                    .Select(e => e.Entity)
                    .Where(o => o.Status == OrderStatus.Deleted
                                && !deletedAndStornedOrderIds.Contains(o.Id))
                    .Do(o => deletedAndStornedOrderIds.Add(o.Id))
                    .Where(StornedTerminalInHostGroup)
                    // .ObserveOn(TaskPoolScheduler.Default)
                    // .Subscribe(y => Task.Run(() => OnOrderStorned(y))
                    .Subscribe(OnOrderStorned
                        , OnError));


            subscriptions.Add(
                PluginContext.Notifications.OrderChanged
                    .Select(e => e.Entity)
                    .Where(o => o.Status == OrderStatus.Closed
                                && !closedOrderIds.Contains(o.Id)
                                && !deletedAndStornedOrderIds.Contains(o.Id))
                    .Do(o => closedOrderIds.Add(o.Id))
                    .Where(LastChangedTerminalInHostGroup)
                    // .ObserveOn(TaskPoolScheduler.Default)
                    // .Subscribe(o => Task.Run(() => OnOrderClosed(o)), OnError));
                    .Subscribe(OnOrderClosed, OnError));


            subscriptions.Add(
                Observable.Timer(TimeSpan.FromMinutes(PluginHelpers.TimerOrderTimeout),
                        TimeSpan.FromMinutes(PluginHelpers.TimerOrderTimeout))
                    .Select(x =>
                        PluginContext.Operations.GetOrders(false, true)
                            .Where(o => o.Status == OrderStatus.Bill
                                        && LastChangedTerminalInHostGroup(o)
                                        && o.BillTime.HasValue
                                        && o.BillTime.Value.AddMinutes(Properties.Settings.Default
                                            .OrderInBillTooLong) < DateTime.Now).ToList()
                    )
                    // .ObserveOn(TaskPoolScheduler.Default)
                    // .Subscribe(o => Task.Run(() => OrdersWithTooLongBillTimeExists(o)), OnError));
                    .Subscribe(OrdersWithTooLongBillTimeExists, OnError));
        }


        private void OnError(Exception ex)
        {
            PluginContext.Log.Error($"Error in OrderChangeNotifier subscription: {ex.Message} ", ex);
        }

        private void OrdersWithTooLongBillTimeExists(List<IOrder> billOrders)
        {
            PluginContext.Log.Debug($"OrdersWithTooLongBillTimeExists :: started.");
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
                        Waiter = o.Waiter?.Name ?? string.Empty,
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

            PluginContext.Log.Debug($"OrdersWithTooLongBillTimeExists :: finished.");
        }


        private bool LastChangedTerminalInHostGroup(IOrder order)
        {
            var terminals = PluginContext.Operations.GetTerminalsGroupTerminals(PluginHelpers.GroupName);
            var terminal = PluginContext.Operations.TryGetTerminalById(order.LastChangedTerminalId);
            if (terminal == null)
            {
                LocalOrder localOrder = null;
                if (_repository != null)
                {
                    localOrder = _repository.GetOrder(order.Id);
                }
                else
                {
                    localOrder = _context.LoadOrder(order);
                }
                var terminalId = localOrder?.LastChangeTerminalId ?? Guid.Empty;
                terminal = PluginContext.Operations.TryGetTerminalById(terminalId);
            }

            return terminals.Contains(terminal);
        }


        private bool StornedTerminalInHostGroup(IOrder order)
        {
            // Таким экзотическим методом дополнительно фильтруем удалённые заказы от возвращаемых.
            if (order.Items.All(i => i.DeletionMethod != null))
                return false;
            return LastChangedTerminalInHostGroup(order);
        }

        private bool DeletedTerminalInHostGroup(IOrder order)
        {
            // Таким экзотическим методом дополнительно фильтруем удалённые заказы от возвращаемых.
            if (order.Items.Any())
            {
                if (order.Items.All(i => i.DeletionMethod == null))
                    return false;
            }

            return LastChangedTerminalInHostGroup(order);
        }

        private void OnOrderClosed(IOrder order)
        {
            if (order is IDeliveryOrder)
                return;
            try
            {
                PluginContext.Log.Info($"OnOrderClosed :: Number {order.Number}");

                LocalOrder oldOrder = null;
                if (_repository != null)
                {
                    oldOrder = _repository.GetOrder(order.Id);
                }
                else
                {
                    oldOrder = _context.LoadOrder(order);
                }
                if (oldOrder?.ShiftCount > 0)
                {
                    PublishEvent(new PluginToServerEvent
                    {
                        PluginEventType = EnumPluginEventType.SeveralOrderShifts,
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
                            OrderShiftCount = oldOrder.ShiftCount,
                        }
                    });
                }


                PublishEvent(new PluginToServerEvent
                {
                    PluginEventType = EnumPluginEventType.ClosingOrder,
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
                PluginContext.Log.Error($"OnOrderClosed :: {ex.Message}", ex);
            }
            finally
            {
                if (_repository != null)
                {
                    _repository.SaveOrder(order, true);
                    _repository.AddHighRiskOperation(order.Waiter, "orderClosed");
                }
                else
                {
                    _context.SaveOrder(order, true);
                    _context.AddHighRiskOperation(order.Waiter, "orderClosed");
                }
            }
        }

        private void OnOrderStorned(IOrder order)
        {
            if (order is IDeliveryOrder)
                return;
            try
            {
                PluginContext.Log.Info($"OnOrderStorned :: Number {order.Number}");
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
                        Waiter = order.Waiter?.Name ?? string.Empty,
                        Cashier = order.Cashier?.Name ?? string.Empty,
                        CloseTime = DateTime.Now,
                        Revenue = order.ResultSum > 0 ? order.ResultSum : null,
                        IsBanquet = order.IsBanquetOrder,
                    }
                });

                if (_repository != null)
                {
                    _repository.SaveOrder(order, true);
                    _repository.AddHighRiskOperation(order.Waiter, "orderVoided");
                }
                else
                {
                    _context.SaveOrder(order, true);
                    _context.AddHighRiskOperation(order.Waiter, "orderVoided");
                }
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error($"OnOrderStorned :: {ex.Message}", ex);
            }
        }

        private void OnOrderDeleting(IOrder order)
        {
            // if (order is IDeliveryOrder)
            //     return;
            try
            {
                if (!LastChangedTerminalInHostGroup(order))
                {
                    return;
                }

                //TODO безобразие!
                if (order.Waiter is null)
                    return;

                PluginContext.Log.Info($"OnOrderDeleting :: Number {order.Number}");
                if (order.StornedOrderId != null)
                    return;


                PublishEvent(new PluginToServerEvent
                {
                    PluginEventType = order is IDeliveryOrder
                        ? EnumPluginEventType.DeliveryOrderCancelled
                        : order.IsBanquetOrder
                            ? EnumPluginEventType.BanquetOrderCancelled
                            :
                            // (order.ResultSum == 0)
                            //     ? EnumPluginEventType.DeleteAnEmptyOrder
                            //     : 
                            EnumPluginEventType.DeletingAnOrder,
                    Data = new PluginToServerEventOrder
                    {
                        Tables = order.Tables.GetTablesAsString(),
                        OrderNum = order.Number,
                        Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                        Waiter = order.Waiter?.Name ?? string.Empty,
                        Cashier = order.Cashier?.Name ?? string.Empty,
                        CloseTime = DateTime.Now,
                        Revenue = order.ResultSum, // > 0 ? order.ResultSum : null,
                        IsBanquet = order.IsBanquetOrder,
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
            }
        }

        private void OnOrderCreating(IOrder order)
        {
            if (order is IDeliveryOrder)
                return;
            var hasChanges = false;
            try
            {
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
                    if (order.StornedOrderId != null)
                        return;
                    PluginContext.Log.Info($"OnOrderCreating :: Number {order.Number}");
                    PublishEvent(new PluginToServerEvent
                    {
                        PluginEventType = EnumPluginEventType.NewOrder,
                        Data = new PluginToServerEventOrder
                        {
                            Tables = order.Tables.GetTablesAsString(),
                            OrderNum = order.Number,
                            Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                            Waiter = order.Waiter?.Name ?? string.Empty,
                            Cashier = order.Cashier?.Name ?? string.Empty,
                            OpenTime = order.OpenTime,
                            //TODO: Только в API8
                            //IsBanquet = order.Reserve != null,
                        }
                    });
                    if (_repository != null)
                    {
                        oldOrder = _repository.SaveOrder(order);
                        _repository.AddHighRiskOperation(order.Waiter, "orderCreated");
                    }
                    else
                    {
                        oldOrder = _context.SaveOrder(order);
                        _context.AddHighRiskOperation(order.Waiter, "orderCreated");
                    }
                }

                // Проверить смену официанта
                if (order.Waiter != null)
                {
                    if (oldOrder.WaiterId != order.Waiter.Id)
                    {
                        PublishEvent(new PluginToServerEvent
                        {
                            PluginEventType = EnumPluginEventType.OrdersWaiterHasChanged,
                            Data = new PluginToServerEventWaiterChanged
                            {
                                Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                                OldWaiterName = oldOrder.WaiterName,
                                NewWaiterName = order.Waiter?.Name ?? string.Empty,
                                OrderNum = order.Number,
                                Revenue = order.ResultSum,
                                Tables = order.Tables.GetTablesAsString(),
                            }
                        });
                        if (_repository != null)
                            _repository.AddHighRiskOperation(order.Waiter, "waiterChanged");
                        else
                            _context.AddHighRiskOperation(order.Waiter, "waiterChanged");
                        hasChanges = true;
                    }
                }

                // Проверить смену столов
                var newTables = LocalOrder.GetTables(order);
                var newTablesTracker =
                    new ChangesTracker<KeyValueLocalOrderClass, KeyValueLocalOrderClass>(oldOrder.Tables, newTables,
                        AreEquals);
                if (newTablesTracker.AddedItems.Any())
                {
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
                                    oldOrder.Tables.Select(x => $"{(int)x.Value.GetValueOrDefault(0)}").ToList() ??
                                    new List<string>()),
                                NewTables = order.Tables.GetTablesAsString(),
                            }
                        });
                        if (_repository != null)
                            _repository.AddHighRiskOperation(order.Waiter, "tableChanged");
                        else
                            _context.AddHighRiskOperation(order.Waiter, "tableChanged");
                    }

                    hasChanges = true;
                }

                // Проверить скидки надбавки
                var newDiscount = LocalOrder.GetDiscountsSurchargesLists(order);

                // Для только что созданного заказа SaveOrder уже записал текущие скидки/надбавки
                // в oldOrder — сравниваем с пустым списком, иначе adding* никогда не сработает.
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
            // var reserves = PluginContext.Operations.GetReserves().Where(x => x.Status != ReserveStatus.Closed);
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
                // Обратная совместимость: если IEventPublisher не передан, событие не будет отправлено
                // В новой архитектуре IEventPublisher всегда должен быть зарегистрирован в DI
                PluginContext.Log.Warn("OrderChangeNotifier :: IEventPublisher not available, event will not be published.");
            }
        }

        public void Dispose()
        {
            subscriptions?.Dispose();
        }
    }
}