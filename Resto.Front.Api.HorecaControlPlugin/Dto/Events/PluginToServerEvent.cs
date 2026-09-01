using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Resto.Front.Api.HorecaControlPlugin.Dto.Converters;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Interfaces;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

/// <summary>
/// ??????? ????? ???????? ??????? ?? ??????
/// </summary>
public class PluginToServerEvent
{
    // ??????? ??????? ??? ? ??????? wire-??????? hc_250305: data ? pluginEventType ? uuid
    [JsonProperty("data", TypeNameHandling = TypeNameHandling.Objects)]
    [JsonConverter(typeof(PluginToServerEventConverter))]
    public IPluginToServerEvent Data { get; set; }

    [JsonProperty("pluginEventType", ItemConverterType = typeof(StringEnumConverter),
        DefaultValueHandling = DefaultValueHandling.Include)]
    public EnumPluginEventType PluginEventType { get; set; }

    [JsonProperty("uuid")] public Guid Uuid { get; set; } = Guid.NewGuid();
}
