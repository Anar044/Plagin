using Resto.Front.Api.Data.Security;
using Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Persistence.Repositories.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Dto;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;
using System;
using System.Collections.Generic;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Application.Services
{
    /// <summary>
    /// Сервис для работы с событиями и высокорисковыми операциями
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IRepository _repository;

        public EventService(IRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        #region Events

        public void AddEvent(PluginToServerEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            PluginContext.Log.Debug($"EventService :: Adding event {evt.PluginEventType}, UUID: {evt.Uuid}");
            _repository.AddEvent(evt);
        }

        public List<PluginToServerEvent> GetUnsentEvents(int batchSize = 100)
        {
            return _repository.GetUnsentEvents(batchSize);
        }

        public void DeleteEvent(Guid uuid)
        {
            if (uuid == Guid.Empty)
                throw new ArgumentException("UUID cannot be empty", nameof(uuid));

            PluginContext.Log.Debug($"EventService :: Deleting event {uuid}");
            _repository.DeleteEvent(uuid);
        }

        public void DeleteAllSentEvents()
        {
            PluginContext.Log.Info("EventService :: Deleting all sent events...");
            _repository.DeleteAllSentEvents();
        }

        #endregion

        #region Messages

        public void AddMessage(PluginEventData message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            PluginContext.Log.Debug($"EventService :: Adding message {message.RequestId}");
            _repository.AddMessage(message);
        }

        public List<PluginEventData> GetUnsentMessages(int batchSize = 100)
        {
            return _repository.GetUnsentMessages(batchSize);
        }

        public void DeleteMessage(Guid uuid)
        {
            if (uuid == Guid.Empty)
                throw new ArgumentException("UUID cannot be empty", nameof(uuid));

            PluginContext.Log.Debug($"EventService :: Deleting message {uuid}");
            _repository.DeleteMessage(uuid);
        }

        public void DeleteAllSentMessages()
        {
            PluginContext.Log.Info("EventService :: Deleting all sent messages...");
            _repository.DeleteAllSentMessages();
        }

        #endregion

        #region High Risk Operations

        public void AddHighRiskOperation(IUser user, string operation)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(operation))
                throw new ArgumentException("Operation cannot be null or empty", nameof(operation));

            PluginContext.Log.Debug($"EventService :: Adding high risk operation '{operation}' for user {user.Name}");
            _repository.AddHighRiskOperation(user, operation);
        }

        public int GetHighRiskOperationsCount(IUser user)
        {
            if (user == null)
                return 0;

            return _repository.GetHighRiskOperationsCount(user);
        }

        public List<HighRiskOperation> GetAllHighRiskOperations()
        {
            return _repository.GetAllHighRiskOperations();
        }

        #endregion
    }
}

