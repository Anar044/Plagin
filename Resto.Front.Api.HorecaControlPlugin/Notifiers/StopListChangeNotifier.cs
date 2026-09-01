using Microsoft.Extensions.DependencyInjection;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace Resto.Front.Api.HorecaControlPlugin.Notifiers;

internal sealed class StopListChangeNotifier : IDisposable
{
    private readonly IDisposable subscription = Disposable.Empty;
    private readonly IEventPublisher _eventPublisher;

    public StopListChangeNotifier(IServiceProvider serviceProvider)
    {
        _eventPublisher = serviceProvider.GetService<IEventPublisher>();

        // Temporarily disabled for iiko Front API 8.4.7010.
        // The legacy GetStopListProductsRemainingAmounts() method is no longer
        // available in this API version and causes MissingMethodException during
        // plugin initialization. Socket.IO and all other plugin functionality
        // must be allowed to start normally.
        PluginContext.Log.Warn(
            "StopListChangeNotifier :: disabled because GetStopListProductsRemainingAmounts() is unavailable in current iiko API.");
    }

    private void PublishEvent(PluginToServerEvent evt)
    {
        if (_eventPublisher != null)
        {
            _eventPublisher.PublishEvent(evt);
        }
        else
        {
            PluginContext.Log.Warn(
                "StopListChangeNotifier :: IEventPublisher not available, event will not be published.");
        }
    }

    public void Dispose()
    {
        subscription.Dispose();
    }
}