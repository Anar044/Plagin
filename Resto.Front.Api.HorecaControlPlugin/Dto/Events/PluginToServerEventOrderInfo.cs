using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Abstracts;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

// TODO: Закончить описание класса

public sealed class PluginToServerEventOrderInfo : APluginToServerEvent
{
    [JsonProperty("isBanquet")]
    public bool IsBanquet { get; set; }

    [JsonProperty("floor")]
    public string Floor { get; set; }

    [JsonProperty("orderNum")] public int OrderNum { get; set; }

    [JsonProperty("tables")]
    public string Tables { get; set; }

    [JsonProperty("waiter")]
    public string Waiter { get; set; }

    [JsonProperty("openTime")]
    public DateTime? OpenTime { get; set; }

    [JsonProperty("billTime")]
    public DateTime? BillTime { get; set; }

    [JsonProperty("discounts")]
    public List<KeyValueClass> Discounts { get; set; }

    [JsonProperty("surcharges")]
    public List<KeyValueClass> Surcharges { get; set; }

    [JsonProperty("tips")]
    public List<KeyValueClass> Tips { get; set; }

    [JsonProperty("estimatedStartTime")]
    public DateTime? EstimatedStartTime { get; set; }


    [JsonProperty("guestsComingTime")]
    public DateTime? GuestsComingTime { get; set; }
}