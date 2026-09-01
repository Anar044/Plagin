using Microsoft.Extensions.DependencyInjection;
using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Data.Common;
using Resto.Front.Api.Data.Kitchen;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

namespace Resto.Front.Api.HorecaControlPlugin.Notifiers;

public class KitchenChangeNotifier : IDisposable
{
    private readonly IEventPublisher _eventPublisher;
    private CompositeDisposable subscriptions = new();

    private List<Guid> alreadyNotified = new();
    private Dictionary<Guid, bool> stateBetweenWaitingOnWay = new();
    private List<Guid> stateWaiting = new();

    public KitchenChangeNotifier(IServiceProvider serviceProvider)
    {
        _eventPublisher = serviceProvider.GetService<IEventPublisher>();

        var readyKitchenOrders = PluginContext.Operations.GetKitchenOrders().Where(z =>
                z.Items.Where(a => !a.Deleted)
                    .All(x => x.ProcessingStatus == KitchenOrderItemProcessingStatus.Processed))
            ?.ToList();
        alreadyNotified = readyKitchenOrders.Select(x => x.Id).ToList();

        PluginContext.Operations.GetDeliveryOrders()
            .Where(x => x.DeliveryStatus == DeliveryStatus.Waiting || x.DeliveryStatus == DeliveryStatus.OnWay ||
                        x.DeliveryStatus == DeliveryStatus.Delivered
                        || x.DeliveryStatus == DeliveryStatus.Closed)
            .ToList().ForEach(x => { stateBetweenWaitingOnWay.Add(x.Id, false); });


        subscriptions.Add(PluginContext.Notifications.KitchenOrderChanged
            .Subscribe(OnKitchenOrderChanged));
    }

    private void OnKitchenOrderChanged(EntityChangedEventArgs<IKitchenOrder> obj)
    {
        try
        {
            var kitchenOrder = obj.Entity;
            var origOrder = PluginContext.Operations.TryGetOrderById(kitchenOrder.BaseOrderId);
            var alreadyInKitchenOrder = alreadyNotified.Contains(kitchenOrder.Id);
            switch (origOrder)
            {
                case IDeliveryOrder deliveryOrder:
                    var cookingCompleted = kitchenOrder?.Items?.Where(d => !d.Deleted)?.All(pr =>
                        pr.ProcessingStatus is KitchenOrderItemProcessingStatus.Processed
                            or KitchenOrderItemProcessingStatus.Served
                    ) ?? false;


                    if (cookingCompleted && !alreadyInKitchenOrder)
                    {
                        var deliveryStatus = deliveryOrder.DeliveryStatus switch
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


                        // //if (deliveryOrder.DeliveryStatus is not (DeliveryStatus.Waiting or DeliveryStatus.OnWay))
                        // if (deliveryOrder.DeliveryStatus is not  DeliveryStatus.OnWay)
                        //     return;
                        //
                        // if (stateBetweenWaitingOnWay.TryGetValue(deliveryOrder.Id, out var value))
                        // {
                        //     if (value)
                        //         return;
                        // }
                        // // else
                        // // {
                        // //     stateBetweenWaitingOnWay.Add(deliveryOrder.Id, false);
                        // // }
                        //
                        // if (deliveryOrder.DeliveryStatus == DeliveryStatus.OnWay)
                        //     stateBetweenWaitingOnWay[deliveryOrder.Id] = true;

                        var processedCookingTime = kitchenOrder.Items?.OrderByDescending(z => z.ProcessingCompleteTime)
                            ?.FirstOrDefault()
                            ?.ProcessingCompleteTime ?? null;


                        var minutes =
                            (int)(processedCookingTime.GetValueOrDefault(DateTime.Now) -
                                  deliveryOrder.PrintTime.GetValueOrDefault(DateTime.Now)
                            ).TotalMinutes;
                        PublishEvent(new PluginToServerEvent
                        {
                            PluginEventType = EnumPluginEventType.DeliveryOrderCookedWaitingForDispatch,
                            Data = new PluginToServerEventOrder
                            {
                                Tables = deliveryOrder.Tables.GetTablesAsString(),
                                OrderNum = deliveryOrder.Number,
                                Floor = deliveryOrder.Tables?.FirstOrDefault()?.RestaurantSection?.Name ?? string.Empty,
                                Waiter = deliveryOrder.DeliveryOperator?.Name ?? string.Empty,
                                Cashier = deliveryOrder.Cashier?.Name ?? string.Empty,
                                OpenTime = deliveryOrder.OpenTime,
                                CloseTime = deliveryOrder.CloseTime,
                                BillTime = deliveryOrder.BillTime,
                                Revenue = deliveryOrder.ResultSum,
                                IsBanquet = false,
                                DeliveryStatus = deliveryStatus,
                                Minutes = minutes,
                                IsDelivery = true
                            },
                        });
                        alreadyNotified.Add(obj.Entity.Id);
                    }

                    // else
                    // {
                    //     alreadyNotified.Remove(obj.Entity.Id);
                    // }
                    break;
                case IOrder order:

                    break;

                case null:
                default:
                    return;
            }
        }
        catch (Exception ex)
        {
            if (Properties.Settings.Default.Debug)
                PluginContext.Log.Error($"OnKitchenOrderChanged :: {ex.Message}");
            else
                PluginContext.Log.Error($"OnKitchenOrderChanged :: {ex.Message}", ex);
        }
    }

    private void PublishEvent(PluginToServerEvent evt)
    {
        if (_eventPublisher != null)
        {
            _eventPublisher.PublishEvent(evt);
        }
        else
        {
            PluginContext.Log.Warn("KitchenChangeNotifier :: IEventPublisher not available, event will not be published.");
        }
    }

    public void Dispose()
    {
        subscriptions?.Dispose();
    }
}