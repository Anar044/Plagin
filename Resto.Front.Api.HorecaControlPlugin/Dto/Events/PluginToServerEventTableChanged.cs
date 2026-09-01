using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Abstracts;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

public sealed class PluginToServerEventTableChanged : APluginToServerEvent
{
    [JsonProperty("waiter")] public string Waiter { get; set; }

    [JsonProperty("oldFloor")]
    public string OldFloor { get; set; }

    [JsonProperty("newFloor")]
    public string NewFloor { get; set; }

    [JsonProperty("oldTables")]
    public string OldTables { get; set; }

    [JsonProperty("newTable")]
    public string NewTables { get; set; }


    [JsonProperty("orderNum")]
    public int OrderNum { get; set; }


    [JsonProperty("revenue")]
    public decimal Revenue { get; set; }
    //
    // [JsonProperty("tables")]
    // public string Tables { get; set; }
}