using Resto.Front.Api.Data.Security;
using Resto.Front.Api.HorecaControlPlugin.Dto;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;
using System;
using System.Collections.Generic;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с событиями и высокорисковыми операциями
    /// </summary>
    public interface IEventService
    {
        #region Events

        /// <summary>
        /// Добавить событие в очередь
        /// </summary>
        void AddEvent(PluginToServerEvent evt);

        /// <summary>
        /// Получить неотправленные события
        /// </summary>
        List<PluginToServerEvent> GetUnsentEvents(int batchSize = 100);

        /// <summary>
        /// Удалить событие
        /// </summary>
        void DeleteEvent(Guid uuid);

        /// <summary>
        /// Удалить все отправленные события
        /// </summary>
        void DeleteAllSentEvents();

        #endregion

        #region Messages

        /// <summary>
        /// Добавить сообщение в очередь
        /// </summary>
        void AddMessage(PluginEventData message);

        /// <summary>
        /// Получить неотправленные сообщения
        /// </summary>
        List<PluginEventData> GetUnsentMessages(int batchSize = 100);

        /// <summary>
        /// Удалить сообщение
        /// </summary>
        void DeleteMessage(Guid uuid);

        /// <summary>
        /// Удалить все отправленные сообщения
        /// </summary>
        void DeleteAllSentMessages();

        #endregion

        #region High Risk Operations

        /// <summary>
        /// Добавить высокорисковую операцию
        /// </summary>
        void AddHighRiskOperation(IUser user, string operation);

        /// <summary>
        /// Получить количество высокорисковых операций для пользователя
        /// </summary>
        int GetHighRiskOperationsCount(IUser user);

        /// <summary>
        /// Получить все высокорисковые операции
        /// </summary>
        List<HighRiskOperation> GetAllHighRiskOperations();

        #endregion
    }
}

