using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Abstracts;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

/// <summary>
/// Событие добавления/удаления скидки/надавки на товар
/// </summary>
public sealed class PluginToServerEventAddDiscountSurchargeItem : APluginToServerEvent
{
    /// <summary>
    /// Название секции
    /// </summary>
    [JsonProperty("floor")]
    public string Floor { get; set; }

    /// <summary>
    /// Название скидки/надавки
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Сумма скидки/надавки
    /// </summary>
    [JsonProperty("value")]
    public decimal Value { get; set; }

    /// <summary>
    /// Процент скидки/надавки
    /// </summary>
    [JsonProperty("valuePercent")]
    public decimal ValuePercent { get; set; }

    /// <summary>
    /// Номер заказа
    /// </summary>
    [JsonProperty("orderNum")]
    public int OrderNum { get; set; }

    /// <summary>
    /// Официант
    /// </summary>
    [JsonProperty("waiter")]
    public string Waiter { get; set; }

    /// <summary>
    /// Сумма в заказе
    /// </summary>
    [JsonProperty("revenue")]
    public decimal Revenue { get; set; }

    /// <summary>
    /// Список столов
    /// </summary>
    [JsonProperty("tables")]
    public string Tables { get; set; }
}