using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Abstracts;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

public class PluginToServerEventStopListAmountResponse : APluginToServerEvent
{
    [JsonProperty("productName")] public string ProductName { get; set; }
    [JsonProperty("amount")] public decimal Amount { get; set; }
}