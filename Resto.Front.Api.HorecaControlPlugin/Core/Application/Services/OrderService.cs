using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Persistence.Repositories.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Sql;
using System;
using System.Collections.Generic;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Application.Services
{
    /// <summary>
    /// Сервис для работы с заказами
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IRepository _repository;

        public OrderService(IRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public LocalOrder SaveOrder(IOrder order, bool deleted = false)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            PluginContext.Log.Debug($"OrderService :: Saving order {order.Id}, deleted: {deleted}");
            return _repository.SaveOrder(order, deleted);
        }

        public LocalOrder GetOrder(Guid orderId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("Order ID cannot be empty", nameof(orderId));

            return _repository.GetOrder(orderId);
        }

        public List<LocalOrder> GetNonClosedOrders()
        {
            return _repository.GetNonClosedOrders();
        }

        public List<LocalOrder> GetNonClosingOrdersOnShiftOpen()
        {
            return _repository.GetNonClosingOrdersOnShiftOpen();
        }

        public void LoadOrders()
        {
            PluginContext.Log.Info("OrderService :: Loading orders...");
            _repository.LoadOrders();
            PluginContext.Log.Info("OrderService :: Orders loaded.");
        }
    }
}

