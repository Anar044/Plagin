using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Abstracts;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

/// <summary>
/// —обытий изменени€ состо€ни€ пользовател€
/// </summary>
public class PluginToServerEventUserState : APluginToServerEvent
{
    [JsonProperty("employeeName")] public string EmployeeName { get; set; }
    [JsonProperty("isSessionOpen")] public bool IsSessionOpen { get; set; }
}