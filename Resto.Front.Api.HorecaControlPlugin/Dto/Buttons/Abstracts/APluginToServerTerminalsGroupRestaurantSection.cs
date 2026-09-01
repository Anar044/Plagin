using System;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Abstracts;

/// <summary>
/// Абстракция для ресторанных секций
/// </summary>
public abstract class APluginToServerTerminalsGroupRestaurantSection //: IPluginToServerTerminalGroupRestaurantSection
{
    [JsonProperty("restaurantSectionId")] public Guid Id { get; set; }

    [JsonProperty("restaurantSectionName")]
    public string Name { get; set; }

    [JsonProperty("totalTables")]
    public int TotalTables { get; set; }
}