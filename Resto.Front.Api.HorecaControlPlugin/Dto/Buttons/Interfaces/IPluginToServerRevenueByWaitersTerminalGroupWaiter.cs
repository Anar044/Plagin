using System;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Interfaces;

/// <summary>
/// Интерфес для официантов
/// </summary>
public interface IPluginToServerRevenueByWaitersTerminalGroupWaiter
{
    /// <summary>
    /// Id официанта
    /// </summary>
    [JsonProperty("waiterId")]
    public Guid WaiterId { get; set; }

    /// <summary>
    /// Имя официанта
    /// </summary>
    [JsonProperty("waiterName")]
    public string WaiterName { get; set; }

    /// <summary>
    /// Количество гостей
    /// </summary>
    [JsonProperty("numberOfGuest")]
    public int NumberOfGuest { get; set; }

    /// <summary>
    /// Количество высокорсиковых операций
    /// </summary>
    [JsonProperty("highRiskOperations")]
    public int HighRiskOperations { get; set; }

    /// <summary>
    /// Количество открытых заказов
    /// </summary>
    [JsonProperty("openOrders")]
    public int OpenedOrders { get; set; }

    /// <summary>
    /// Количество закрытых заказов
    /// </summary>
    [JsonProperty("closedOrders")]
    public int ClosedOrders { get; set; }

    /// <summary>
    /// Ожидаемая сумма открытых и пречековых заказов
    /// </summary>
    [JsonProperty("openOrdersMoneySum")]
    public decimal OpenOrdersMoneySum { get; set; }

    /// <summary>
    /// Сумма закрытых заказов
    /// </summary>
    [JsonProperty("closedOrdersMoneySum")]
    public decimal ClosedOrdersMoneySum { get; set; }
}