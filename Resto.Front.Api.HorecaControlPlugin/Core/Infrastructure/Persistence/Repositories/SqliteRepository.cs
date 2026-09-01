using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Persistence.Repositories.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Dto;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using Resto.Front.Api.HorecaControlPlugin.Sql;
using Resto.Front.Api.HorecaControlPlugin.Sqlite;
using Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Реализация репозитория на основе SQLite
    /// </summary>
    public class SqliteRepository : IRepository
    {
        private readonly HorecaSqlite _db;

        public SqliteRepository(HorecaSqlite db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        #region Orders

        public LocalOrder GetOrder(Guid orderId)
        {
            return _db.LoadOrder(orderId);
        }

        public LocalOrder SaveOrder(IOrder order, bool deleted = false)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return _db.SaveOrder(order, deleted);
        }

        public List<LocalOrder> GetNonClosedOrders()
        {
            var orders = PluginContext.Operations.GetOrders();
            return orders
                .Where(o => o.Status != OrderStatus.Closed && o.Status != OrderStatus.Deleted)
                .Select(o => _db.LoadOrder(o))
                .Where(lo => lo != null)
                .ToList();
        }

        public void LoadOrders()
        {
            // HorecaSqlite.LoadLocalOrders() - приватный метод
            // Используем OnStart, который вызывает LoadLocalOrders
            _db.OnStart();
        }

        public List<LocalOrder> GetNonClosingOrdersOnShiftOpen()
        {
            return _db.ShiftOpenLoadNonClosingOrders();
        }

        #endregion

        #region Events

        public void AddEvent(PluginToServerEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            try
            {
                _db.AddEvent(evt);
            }
            catch (ObjectDisposedException ex)
            {
                PluginContext.Log.Warn($"SqliteRepository.AddEvent :: Database is disposed, cannot add event: {ex.Message}");
                throw;
            }
        }

        public List<PluginToServerEvent> GetEvents()
        {
            return _db.GetEvents();
        }

        public List<PluginToServerEvent> GetUnsentEvents(int batchSize = 100)
        {
            return _db.GetEvents()
                .Where(e => e != null)
                .Take(batchSize)
                .ToList();
        }

        public void DeleteEvent(Guid uuid)
        {
            _db.DeleteEvent(uuid);
        }

        public void DeleteAllSentEvents()
        {
            _db.DeleteAllSentEvents();
        }

        #endregion

        #region Messages

        public void AddMessage(PluginEventData message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _db.AddMessage(message);
        }

        public List<PluginEventData> GetMessages()
        {
            return _db.GetMessages();
        }

        public List<PluginEventData> GetUnsentMessages(int batchSize = 100)
        {
            return _db.GetMessages()
                .Where(m => m != null)
                .Take(batchSize)
                .ToList();
        }

        public void DeleteMessage(Guid uuid)
        {
            _db.DeleteMessage(uuid);
        }

        public void DeleteAllSentMessages()
        {
            _db.DeleteAllSentMessages();
        }

        #endregion

        #region Shifts

        public Shift GetOpenShift()
        {
            _db.GetOpenShift();
            return _db.Shift;
        }

        public void OpenShift(DateTime openTime, IUser user)
        {
            _db.OpenShift(openTime, user);
        }

        public void CloseShift(IUser user)
        {
            _db.CloseShift(user);
        }

        public void IncrementShiftCountForOpenOrders()
        {
            _db.IncrementCountOrder();
        }

        #endregion

        #region High Risk Operations

        public void AddHighRiskOperation(IUser user, string operation)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(operation))
                throw new ArgumentException("Operation cannot be null or empty", nameof(operation));

            _db.AddHighRiskOperation(user, operation);
        }

        public int GetHighRiskOperationsCount(IUser user)
        {
            if (user == null)
                return 0;

            return _db.GetHighRiskOperations(user);
        }

        public List<HighRiskOperation> GetAllHighRiskOperations()
        {
            var list = _db.HighRiskOperationList?.ToList() ?? new List<HighRiskOperation>();
            if (_db.Shift == null)
                return new List<HighRiskOperation>();

            return list.Where(x => x.ShiftId == _db.Shift.Id).ToList();
        }

        #endregion

        #region Users

        public User GetOrCreateUser(IUser iikoUser)
        {
            if (iikoUser == null)
                return null;

            // HorecaSqlite.GetUser() - приватный метод
            // Используем прямой доступ к Users через ITable для получения существующего пользователя
            var existingUser = _db.Users.FirstOrDefault(u => u.UserId == iikoUser.Id);
            if (existingUser != null)
                return existingUser;

            // Если пользователя нет, он будет автоматически создан при вызове других методов
            // (AddHighRiskOperation, OpenShift и т.д.), которые используют приватный GetUser
            // Возвращаем null, так как создание происходит через внутренние механизмы HorecaSqlite
            // TODO: Создать публичный метод GetOrCreateUser в HorecaSqlite
            return null;
        }

        #endregion

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}

