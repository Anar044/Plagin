using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.HorecaControlPlugin.Dto;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using Resto.Front.Api.HorecaControlPlugin.Sql;
using Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;
using System;
using System.Collections.Generic;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Persistence.Repositories.Interfaces
{
    /// <summary>
    /// Интерфейс репозитория для работы с данными (синхронный)
    /// </summary>
    public interface IRepository : IDisposable
    {
        #region Orders

        /// <summary>
        /// Получить заказ по ID
        /// </summary>
        LocalOrder GetOrder(Guid orderId);

        /// <summary>
        /// Сохранить заказ
        /// </summary>
        LocalOrder SaveOrder(IOrder order, bool deleted = false);

        /// <summary>
        /// Получить незакрытые заказы
        /// </summary>
        List<LocalOrder> GetNonClosedOrders();

        /// <summary>
        /// Загрузить заказы при старте
        /// </summary>
        void LoadOrders();

        /// <summary>
        /// Получить незакрытые заказы при открытии смены
        /// </summary>
        List<LocalOrder> GetNonClosingOrdersOnShiftOpen();

        #endregion

        #region Events

        /// <summary>
        /// Добавить событие в очередь
        /// </summary>
        void AddEvent(PluginToServerEvent evt);

        /// <summary>
        /// Получить неотправленные события
        /// </summary>
        List<PluginToServerEvent> GetEvents();

        /// <summary>
        /// Получить неотправленные события (с ограничением размера батча)
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
        List<PluginEventData> GetMessages();

        /// <summary>
        /// Получить неотправленные сообщения (с ограничением размера батча)
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

        #region Shifts

        /// <summary>
        /// Получить открытую смену
        /// </summary>
        Shift GetOpenShift();

        /// <summary>
        /// Открыть смену
        /// </summary>
        void OpenShift(DateTime openTime, IUser user);

        /// <summary>
        /// Закрыть смену
        /// </summary>
        void CloseShift(IUser user);

        /// <summary>
        /// Увеличить счетчик смен для открытых заказов
        /// </summary>
        void IncrementShiftCountForOpenOrders();

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

        #region Users

        /// <summary>
        /// Получить или создать пользователя
        /// </summary>
        User GetOrCreateUser(IUser iikoUser);

        #endregion
    }
}

