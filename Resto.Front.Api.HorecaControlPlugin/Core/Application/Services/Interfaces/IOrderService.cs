using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.HorecaControlPlugin.Sql;
using System;
using System.Collections.Generic;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с заказами
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Сохранить заказ
        /// </summary>
        LocalOrder SaveOrder(IOrder order, bool deleted = false);

        /// <summary>
        /// Получить заказ по ID
        /// </summary>
        LocalOrder GetOrder(Guid orderId);

        /// <summary>
        /// Получить незакрытые заказы
        /// </summary>
        List<LocalOrder> GetNonClosedOrders();

        /// <summary>
        /// Получить незакрытые заказы при открытии смены
        /// </summary>
        List<LocalOrder> GetNonClosingOrdersOnShiftOpen();

        /// <summary>
        /// Загрузить заказы при старте
        /// </summary>
        void LoadOrders();
    }
}

