using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Abstracts;

/// <summary>
/// Абстракция для группы терминалов
/// </summary>
/// <typeparam name="RS"></typeparam>
public abstract class APluginToServerTerminalsGroup<RS> where RS : APluginToServerTerminalsGroupRestaurantSection
{
    [JsonProperty("terminalsGroupId")] public Guid Id { get; set; }
    [JsonProperty("terminalsGroupName")] public string Name { get; set; }

    [JsonProperty("totalTables")]
    public int TotalTables { get; set; }

    [JsonProperty("restaurantSections")] public virtual List<RS> RestaurantSections { get; set; }
}