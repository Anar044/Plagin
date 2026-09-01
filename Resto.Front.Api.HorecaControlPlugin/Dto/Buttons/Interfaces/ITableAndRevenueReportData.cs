using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Interfaces;

/// <summary>
/// Интерфейс для хранения данных по столам и операциям
/// </summary>
public interface ITableAndRevenueReportData
{
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
    /// Ожидаемое сумма с открытых и пречековых заказов 
    /// </summary>
    [JsonProperty("expectedRevenueMoneySum")]
    public decimal ExpectedRevenueMoneySum { get; set; }

    /// <summary>
    /// Сумма с открытых заказов
    /// </summary>
    [JsonProperty("openOrdersMoneySum")]
    public decimal OpenOrdersMoneySum { get; set; }

    /// <summary>
    /// Сумма закрытых заказов
    /// </summary>
    [JsonProperty("closedOrdersMoneySum")]
    public decimal ClosedOrdersMoneySum { get; set; }

    /// <summary>
    /// Количество столов с открытыми заказами
    /// </summary>
    [JsonProperty("activeTables")]
    public int ActiveTables { get; set; }

    /// <summary>
    /// Количество столов с без заказов
    /// </summary>
    [JsonProperty("inActiveTables")]
    public int InActiveTables { get; set; }

    /// <summary>
    /// Банкетов сегодня
    /// </summary>
    [JsonProperty("banquetsToday")]
    public int BanquetsToday { get; set; }

    /// <summary>
    /// Банкетов на неделе
    /// </summary>
    [JsonProperty("banquetsWeek")]
    public int BanquetsWeek { get; set; }

    /// <summary>
    /// Банкетов на месяц
    /// </summary>
    [JsonProperty("banquetsMonth")]
    public int BanquetsMonth { get; set; }

    /// <summary>
    /// Коэффициент загруженности столов
    /// </summary>
    [JsonProperty("workLoad")]
    public decimal WorkLoad { get; set; }

    /// <summary>
    /// Количество гостей
    /// </summary>
    [JsonProperty("numberOfGuest")]
    public int NumberOfGuest { get; set; }

    /// <summary>
    /// Количество заказов
    /// </summary>
    [JsonProperty("orderCount")]
    public int OrderCount { get; set; }

    /// <summary>
    /// Список скидок
    /// </summary>
    [JsonProperty("discounts")]
    public List<KeyValueClass> Discounts { get; set; }

    /// <summary>
    /// Список надбавок
    /// </summary>
    [JsonProperty("surcharges")]
    public List<KeyValueClass> Surcharges { get; set; }

    /// <summary>
    /// Список оплат
    /// </summary>
    [JsonProperty("payments")]
    public List<KeyValueClass> Payments { get; set; }

    /// <summary>
    /// Список чаевых
    /// </summary>
    [JsonProperty("tips")]
    public List<KeyValueClass> Tips { get; set; }

    /// <summary>
    /// Количество открытых банкетных заказов
    /// </summary>
    [JsonProperty("openedBanquetOrders")]
    public int OpenedBanquetOrders { get; set; }

    /// <summary>
    /// Ожидаемая сумма в открытых банкетных заказах
    /// </summary>
    [JsonProperty("openBanquetOrdersMoneySum")]
    public decimal OpenBanquetOrdersMoneySum { get; set; }

    /// <summary>
    /// Ожидаемая сумма в открытых и пречековых банкетных заказах
    /// </summary>
    [JsonProperty("expectedBanquetRevenueMoneySum")]
    public decimal ExpectedBanquetRevenueMoneySum { get; set; }

    /// <summary>
    /// Количество столов с банкетами
    /// </summary>
    [JsonProperty("activeBanquetTables")]
    public int ActiveBanquetTables { get; set; }

    /// <summary>
    /// Количество гостей в банкетах
    /// </summary>
    [JsonProperty("banquetNumberOfGuest")]
    public int BanquetNumberOfGuest { get; set; }

    /// <summary>
    /// Количество закрытых заказов в банкетах
    /// </summary>
    [JsonProperty("closedBanquetOrders")]
    public int ClosedBanquetOrders { get; set; }

    /// <summary>
    /// Сумма в закрытых банкетных заказах
    /// </summary>
    [JsonProperty("closedBanquetOrdersMoneySum")]
    public decimal ClosedBanquetOrdersMoneySum { get; set; }
}