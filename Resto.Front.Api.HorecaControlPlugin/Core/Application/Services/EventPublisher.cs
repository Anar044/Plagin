using Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Communication.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Application.Services
{
    /// <summary>
    /// Публикатор событий
    /// </summary>
    public class EventPublisher : IEventPublisher
    {
        private readonly ISocketConnectionManager _connectionManager;

        public EventPublisher(ISocketConnectionManager connectionManager)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        }

        public void PublishEvent(PluginToServerEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            _connectionManager.SendEvent(evt);
        }

        public void SendUnsentEvents()
        {
            _connectionManager.SendUnsentEvents();
        }
    }
}

