using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Abstracts;
using System;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

/// <summary>
/// Событие открытия и закрытия смены
/// </summary>
public sealed class PluginToServerEventOpenCloseSession : APluginToServerEvent
{
    /// <summary>
    /// Время открытия смены
    /// </summary>
    [JsonProperty("openTime")]
    public DateTime? OpenTime { get; set; }

    /// <summary>
    /// Время закрытия смены
    /// </summary>
    [JsonProperty("closeTime")]
    public DateTime? CloseTime { get; set; }

    /// <summary>
    /// Сумма прихода при закрытии смены
    /// </summary>
    [JsonProperty("revenue")]
    public decimal? Revenue { get; set; }

    /// <summary>
    /// Номер смены в системе
    /// </summary>
    [JsonProperty("shiftNumber")]
    public int? ShiftNumber { get; set; }
}