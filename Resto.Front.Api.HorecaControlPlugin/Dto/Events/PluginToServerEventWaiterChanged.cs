using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Abstracts;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

/// <summary>
/// Событие изменен официант
/// </summary>
public sealed class PluginToServerEventWaiterChanged : APluginToServerEvent
{
    [JsonProperty("floor")]
    public string Floor { get; set; }

    [JsonProperty("oldWaiterName")]
    public string OldWaiterName { get; set; }

    [JsonProperty("newWaiterName")]
    public string NewWaiterName { get; set; }


    [JsonProperty("orderNum")]
    public int OrderNum { get; set; }


    [JsonProperty("revenue")]
    public decimal Revenue { get; set; }

    [JsonProperty("tables")]
    public string Tables { get; set; }
}