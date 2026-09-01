using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

public class PluginToServerRevenueByWaiters : APluginToServer
{
    [JsonProperty("terminalGroup")]
    public List<PluginToServerRevenueByWaitersTerminalGroup> TerminalGroups { get; set; } = new();
}

public class PluginToServerRevenueByWaitersTerminalGroup
{
    [JsonIgnore] public Guid TerminalsGroupId { get; set; }

    [JsonProperty("terminalsGroupName")] public string TerminalsGroupName { get; set; }

    /// <summary>
    /// Список официантов
    /// </summary>
    [JsonProperty("waiters")]
    public List<PluginToServerRevenueByWaitersTerminalGroupWaiter> Waiters { get; set; } = new();
}

public class PluginToServerRevenueByWaitersTerminalGroupWaiter
{
    /// <summary>
    /// Идентификатор официанта
    /// </summary>
    [JsonProperty("waiterId")]
    public Guid WaiterId { get; set; }

    /// <summary>
    /// Имя официанта
    /// </summary>
    [JsonProperty("waiterName")]
    public string WaiterName { get; set; }

    /// <summary>
    /// Количество гостей обслуженных официантом
    /// </summary>

    [JsonProperty("numberOfGuest")]
    public int NumberOfGuest { get; set; }

    /// <summary>
    /// Количество высокорискованных операций официанта
    /// </summary>

    [JsonProperty("highRiskOperations")]
    public int HighRiskOperations { get; set; }

    /// <summary>
    /// Количество открытых операций официанта
    /// </summary>
    [JsonProperty("openOrders")]
    public int OpenedOrders { get; set; }

    /// <summary>
    /// Количество закрытых операций официанта
    /// </summary>
    [JsonProperty("closedOrders")]
    public int ClosedOrders { get; set; }

    /// <summary>
    /// Сумма в открытых операциях официанта
    /// </summary>
    [JsonProperty("openOrdersMoneySum")]
    public decimal OpenOrdersMoneySum { get; set; }

    /// <summary>
    /// Сумма в закрытых операциях официанта
    /// </summary>
    [JsonProperty("closedOrdersMoneySum")]
    public decimal ClosedOrdersMoneySum { get; set; }
}