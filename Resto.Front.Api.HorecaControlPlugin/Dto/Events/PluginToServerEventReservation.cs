using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Abstracts;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

// TODO: Закончить описание класса

public class PluginToServerEventReservation : APluginToServerEvent
{
    [JsonProperty("clientName")]
    public string ClientName { get; set; }

    [JsonProperty("phone")]
    public string Phone { get; set; }

    [JsonProperty("floor")]
    public string Floor { get; set; }


    [JsonProperty("tables")]
    public string Tables { get; set; }

    [JsonProperty("reason")]
    public string Reason { get; set; }

    [JsonProperty("comment")]
    public string Comment { get; set; }
}