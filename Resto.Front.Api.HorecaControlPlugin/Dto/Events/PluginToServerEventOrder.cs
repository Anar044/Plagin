using Newtonsoft.Json;
using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Abstracts;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

// TODO: ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
public sealed class PluginToServerEventOrder : APluginToServerEvent
{
    [JsonProperty("tables", NullValueHandling = NullValueHandling.Include)]
    public string Tables { get; set; }

    [JsonProperty("orderNum")] public int OrderNum { get; set; }

    [JsonProperty("floor")] public string Floor { get; set; }
    [JsonProperty("orderShiftCount", NullValueHandling = NullValueHandling.Include)] public int? OrderShiftCount { get; set; }
    [JsonProperty("waiter")] public string Waiter { get; set; }
    [JsonProperty("cashier")] public string Cashier { get; set; }

    [JsonProperty("openTime", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? OpenTime { get; set; }

    [JsonProperty("closeTime", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? CloseTime { get; set; }

    [JsonProperty("minutes", NullValueHandling = NullValueHandling.Ignore)]
    public int? Minutes { get; set; }

    [JsonProperty("billTime", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? BillTime { get; set; }

    [JsonProperty("revenue", NullValueHandling = NullValueHandling.Ignore)]
    public decimal? Revenue { get; set; }

    [JsonProperty("isBanquet", DefaultValueHandling = DefaultValueHandling.Include)]
    public bool IsBanquet { get; set; }

    [JsonProperty("totalMinutesBetweenBillAndClose", DefaultValueHandling = DefaultValueHandling.Include,
        NullValueHandling = NullValueHandling.Ignore)]
    public int? TotalMinutesBetweenBillAndClose { get; set; }

    [JsonProperty("orderInBillTooLong", DefaultValueHandling = DefaultValueHandling.Include,
        NullValueHandling = NullValueHandling.Ignore)]
    public int? OrderInBillTooLong { get; set; }

    [JsonProperty("deliveryStatus", DefaultValueHandling = DefaultValueHandling.Include,
        NullValueHandling = NullValueHandling.Ignore)]
    public EnumDeliveryOrderStatusDto? DeliveryStatus { get; set; }

    [JsonProperty("isDelivery", DefaultValueHandling = DefaultValueHandling.Include,
        NullValueHandling = NullValueHandling.Ignore)]
    public bool IsDelivery { get; set; } = false;
}
