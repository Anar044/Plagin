using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Interfaces;
using System.Collections.Generic;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces
{
    /// <summary>
    /// Интерфейс для генерации отчетов
    /// </summary>
    public interface IReportGenerator
    {
        /// <summary>
        /// Генерация сводного отчета по ресторану
        /// </summary>
        IPluginToServer GenerateSummaryReport(List<IOrder> orders, int activeEmployees);

        /// <summary>
        /// Генерация отчета по выручке по этажам
        /// </summary>
        IPluginToServer GenerateRevenueByFloorsReport(List<IOrder> orders, int activeEmployees);

        /// <summary>
        /// Генерация отчета по официантам
        /// </summary>
        IPluginToServer GenerateWaitersReport(List<IOrder> orders);

        /// <summary>
        /// Генерация списка заказов текущей смены
        /// </summary>
        IPluginToServer GenerateCurrentShiftOrders(List<IOrder> orders, List<IReserve> reserves);

        /// <summary>
        /// Генерация отчета ТОП-10 блюд по выручке
        /// </summary>
        IPluginToServer GenerateTopTenMealsReport(List<IOrder> orders);

        /// <summary>
        /// Генерация отчета по стоп-листу
        /// </summary>
        IPluginToServer GenerateStopListReport(Dictionary<IProduct, decimal> products);

        /// <summary>
        /// Генерация отчета по столам с открытыми заказами
        /// </summary>
        IPluginToServer GenerateTablesWithOpenOrders(List<IOrder> orders);

        /// <summary>
        /// Генерация отчета по высокорисковым операциям
        /// </summary>
        IPluginToServer GenerateHighRiskOperationsReport();

        /// <summary>
        /// Генерация детализации по заказу
        /// </summary>
        IPluginToServer GenerateOrderDetails(int orderNumber, List<IOrder> orders, List<IReserve> reserves);
    }
}

