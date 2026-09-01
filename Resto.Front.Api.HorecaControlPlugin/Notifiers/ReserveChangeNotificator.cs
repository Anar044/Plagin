using Microsoft.Extensions.DependencyInjection;
using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Data.Common;
using Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Persistence.Repositories.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using Resto.Front.Api.HorecaControlPlugin.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Resto.Front.Api.HorecaControlPlugin.Notifiers
{
    internal class ReserveChangeNotificator : IDisposable
    {
        private readonly CompositeDisposable subscriptions = new CompositeDisposable();
        private readonly IRepository _repository;
        private readonly IEventPublisher _eventPublisher;
        private readonly HorecaSqlite _context; // Для обратной совместимости

        public ReserveChangeNotificator(IServiceProvider serviceProvider)
        {
            _eventPublisher = serviceProvider.GetService<IEventPublisher>();
            _repository = serviceProvider.GetService<IRepository>();
            _context = serviceProvider.GetRequiredService<HorecaSqlite>();
            subscriptions.Add(PluginContext.Notifications.ReserveChanged
                .Where(s =>
                    s.EventType == EntityEventType.Created
                    && s.Entity.Order == null
                )
                .Subscribe(x => OnBanquetCreated(x.Entity)));

            subscriptions.Add(PluginContext.Notifications.ReserveChanged
                .Where(e => e.Entity.CancelReason != null)
                .Subscribe(x => OnBanquetCanceled(x.Entity)));

            subscriptions.Add(PluginContext.Notifications.ReserveChanged
                .Where(e => e.Entity.CancelReason != null
                            && e.Entity.Order != null
                )
                .Subscribe(x => OnBanquetStarted(x.Entity)));
        }

        private void OnBanquetStarted(IReserve objEntity)
        {
        }

        private void OnBanquetCanceled(IReserve reserve)
        {
            try
            {
                PluginContext.Log.Info($"OnBanquetCanceled :: ");

                var listTables = new List<string>();
                if (reserve.Tables != null && reserve.Tables.Any())
                {
                    foreach (var reserveTable in reserve.Tables)
                    {
                        listTables.Add(reserveTable.Number.ToString());
                    }
                }

                var phone = string.Empty;
                if (reserve.Client?.Phones != null && reserve.Client.Phones.Any())
                {
                    phone = reserve.Client?.Phones.FirstOrDefault()?.Value ?? string.Empty;
                }

                PublishEvent(new PluginToServerEvent
                {
                    PluginEventType = EnumPluginEventType.ReserveIsCancelled,
                    Data = new PluginToServerEventReservation
                    {
                        Floor = reserve.Tables?.FirstOrDefault()?.RestaurantSection?.Name ?? string.Empty,
                        Reason = reserve.CancelReason.ToString(),
                        Comment = reserve.Comment,
                        Tables = string.Join(", ", listTables),
                        ClientName =
                            $"{reserve.Client?.Name ?? string.Empty} {reserve.Client?.Surname ?? string.Empty}",
                        Phone = phone,
                    }
                });
                if (_repository != null)
                    _repository.AddHighRiskOperation(PluginContext.Operations.GetCurrentUser(), "reserveOrBanquetCancelled");
                else
                    _context.AddHighRiskOperation(PluginContext.Operations.GetCurrentUser(), "reserveOrBanquetCancelled");
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error($"OnReserveCancelled :: {ex.Message}", ex);
            }
        }

        private void OnBanquetCreated(IReserve reserve)
        {
            try
            {
                PluginContext.Log.Info($"OnBanquetCreated :: started");

                var phone = string.Empty;
                if (reserve.Client.Phones != null && reserve.Client.Phones.Any())
                {
                    phone = reserve.Client.Phones[0].Value;
                }

                var listTables = new List<string>();
                if (reserve.Tables != null && reserve.Tables.Any())
                {
                    foreach (var reserveTable in reserve.Tables)
                    {
                        listTables.Add(reserveTable.Number.ToString());
                    }
                }

                PublishEvent(new PluginToServerEvent
                {
                    PluginEventType = EnumPluginEventType.NewBanquetOrReservation,
                    Data = new PluginToServerEventReservation
                    {
                        Floor = reserve.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
                        ClientName = $"{reserve.Client.Name} {reserve.Client.Surname}",
                        Phone = phone,
                        Tables = string.Join(", ", listTables)
                    }
                });
                PluginContext.Log.Info($"OnBanquetCreated :: finished");
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error($"OnBanquetCreated :: {ex.Message}", ex);
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
                PluginContext.Log.Warn("ReserveChangeNotificator :: IEventPublisher not available, event will not be published.");
            }
        }

        public void Dispose()
        {
            subscriptions?.Dispose();
        }
    }
}