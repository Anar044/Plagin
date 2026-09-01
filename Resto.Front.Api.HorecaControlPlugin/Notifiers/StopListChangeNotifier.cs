using Microsoft.Extensions.DependencyInjection;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Resto.Front.Api.HorecaControlPlugin.Notifiers;

internal sealed class StopListChangeNotifier : IDisposable
{
    private static readonly IEqualityComparer<KeyValuePair<IProduct, decimal>> RemainingsComparer =
        new ProductsComparer();

    private readonly IDisposable subscription;
    private readonly IEventPublisher _eventPublisher;

    public StopListChangeNotifier(IServiceProvider serviceProvider)
    {
        _eventPublisher = serviceProvider.GetService<IEventPublisher>();


#if V8P5
        var remainingAmounts = PluginContext.Operations.GetProductsRemainingAmounts();
        subscription = PluginContext.Notifications.ProductsRemainingAmountsChanged
            .Select(_ => PluginContext.Operations.GetProductsRemainingAmounts())

#else
        var remainingAmounts = PluginContext.Operations
            .GetStopListProductsRemainingAmounts().ToDictionary(x => x.Key.Product, x => x.Value);
        subscription = PluginContext.Notifications.StopListProductsRemainingAmountsChanged
            .Select(_ =>
                PluginContext.Operations.GetStopListProductsRemainingAmounts()
                    .ToDictionary(x => x.Key.Product, x => x.Value))
#endif

            .Select(currentAmounts => new
            {
                AddedProducts = currentAmounts
                    .Except(remainingAmounts, RemainingsComparer)
                    .ToList(),
                ChangedProducts = currentAmounts
                    .Intersect(remainingAmounts, RemainingsComparer)
                    .Select(x => new { Product = x.Key, AmountDiff = x.Value + remainingAmounts[x.Key] })
                    .Where(x => x.AmountDiff != 0m)
                    .ToList(),
                DeletedProducts = remainingAmounts
                    .Except(currentAmounts, RemainingsComparer)
                    .ToList(),
                RemainingAmounts = currentAmounts
            })
            .Do(changes => remainingAmounts = changes.RemainingAmounts)
            .Select(changes => changes)
            .Subscribe(changes =>
            {
                if (changes.AddedProducts.Any())
                {
                    changes.AddedProducts.ForEach(x =>
                    {
                        PublishEvent(new PluginToServerEvent
                        {
                            PluginEventType = EnumPluginEventType.ChangeItemsAmountOnStopList,
                            Data = new PluginToServerEventStopListAmountResponse
                            {
                                ProductName = x.Key.Name,
                                Amount = x.Value,
                            },
                        });
                    });
                }

                if (changes.ChangedProducts.Any())
                {
                    changes.ChangedProducts.ForEach(x =>
                    {
                        PublishEvent(new PluginToServerEvent
                        {
                            PluginEventType = EnumPluginEventType.ChangeItemsAmountOnStopList,
                            Data = new PluginToServerEventStopListAmountResponse
                            {
                                ProductName = x.Product.Name,
                                Amount = x.AmountDiff,
                            },
                        });
                    });
                }

                if (changes.DeletedProducts.Any())
                {
                    changes.DeletedProducts.ForEach(x =>
                    {
                        PublishEvent(new PluginToServerEvent
                        {
                            PluginEventType = EnumPluginEventType.RemoveFromStopList,
                            Data = new PluginToServerEventStopListAmountResponse
                            {
                                ProductName = x.Key.Name,
                                Amount = x.Value,
                            },
                        });
                    });
                }
            });
    }

    private void PublishEvent(PluginToServerEvent evt)
    {
        if (_eventPublisher != null)
        {
            _eventPublisher.PublishEvent(evt);
        }
        else
        {
            PluginContext.Log.Warn("StopListChangeNotifier :: IEventPublisher not available, event will not be published.");
        }
    }

    public void Dispose()
    {
        subscription.Dispose();
    }

    private sealed class ProductsComparer : IEqualityComparer<KeyValuePair<IProduct, decimal>>
    {
        public bool Equals(KeyValuePair<IProduct, decimal> x, KeyValuePair<IProduct, decimal> y)
        {
            return Equals(x.Key, y.Key);
        }

        public int GetHashCode(KeyValuePair<IProduct, decimal> obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}