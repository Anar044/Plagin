using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Abstracts;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

/// <summary>
/// Событие удаления отпечатанного/неотпечатанного товара
/// </summary>
public sealed class PluginToServerEventDeletionPrintedItem : APluginToServerEvent
{
    /// <summary>
    /// Номер стола
    /// </summary>
    [JsonProperty("tables", NullValueHandling = NullValueHandling.Include)]
    public string Tables { get; set; }

    /// <summary>
    /// Номер заказа
    /// </summary>
    [JsonProperty("orderNum")]
    public int OrderNum { get; set; }

    /// <summary>
    /// номер секции
    /// </summary>
    [JsonProperty("floor")]
    public string Floor { get; set; }

    /// <summary>
    /// Официант
    /// </summary>
    [JsonProperty("waiter")]
    public string Waiter { get; set; }

    /// <summary>
    /// Кассир
    /// </summary>
    [JsonProperty("cashier")]
    public string Cashier { get; set; }

    /// <summary>
    /// Наименование товара
    /// </summary>
    [JsonProperty("productName")]
    public string ProductName { get; set; }

    /// <summary>
    /// Стоимость
    /// </summary>
    [JsonProperty("sum")]
    public decimal Sum { get; set; }

    /// <summary>
    /// Причина списания
    /// </summary>
    [JsonProperty("reasonWriteOff", NullValueHandling = NullValueHandling.Ignore)]
    public string ReasonWriteOff { get; set; }

    /// <summary>
    /// Комментарий по списанию
    /// </summary>
    [JsonProperty("reasonComment", NullValueHandling = NullValueHandling.Ignore)]
    public string ReasonComment { get; set; }

    /// <summary>
    /// Тип товара
    /// </summary>
    [JsonProperty("productType", DefaultValueHandling = DefaultValueHandling.Include,
        ItemConverterType = typeof(StringEnumConverter))]
    public EnumProductType ProductType { get; set; }
}
