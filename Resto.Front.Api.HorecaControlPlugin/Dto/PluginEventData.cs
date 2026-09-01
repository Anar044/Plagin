using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;
using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Dto.Converters;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Dto;

public class PluginFullData
{
    /// <summary>
    /// ID квитанции
    /// </summary>
    [JsonProperty("requestId")]
    public Guid RequestId { get; set; }

    /// <summary>
    /// данные для ответа
    /// </summary>
    [JsonProperty("data", TypeNameHandling = TypeNameHandling.Objects)]
    [JsonConverter(typeof(PluginToServerEventConverter))]
    public IPluginToServer Data { get; set; }
}

public class PluginEventData
{
    /// <summary>
    /// менять нельзя
    /// </summary>
    [JsonProperty("chatId")]
    public string ChatId { get; set; }

    /// <summary>
    /// менять нельзя
    /// </summary>
    [JsonProperty("requestId")]
    public string RequestId { get; set; }

    /// <summary>
    /// тип запроса - менять нельзя
    /// </summary>
    [JsonProperty("requestType", ItemConverterType = typeof(StringEnumConverter),
        DefaultValueHandling = DefaultValueHandling.Include)]
    public EnumRequestType? RequestType { get; set; }


    [JsonProperty("requestDetail", DefaultValueHandling = DefaultValueHandling.Include)]
    public string RequestDetail { get; set; }


    /// <summary>
    /// данные для ответа
    /// </summary>
    [JsonProperty("data", TypeNameHandling = TypeNameHandling.Objects)]
    [JsonConverter(typeof(PluginToServerEventConverter))]
    public IPluginToServer Data { get; set; }

    [JsonProperty("uuid")] public Guid Uuid { get; set; }
}
