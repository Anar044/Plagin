using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

public class PluginToServerStopListRemainingMeals : APluginToServer
{
    /// <summary>
    /// Группа терминалов
    /// </summary>
    [JsonProperty("terminalsGroup")]
    public string TerminalsGroup { get; set; } = PluginHelpers.GroupName.Name;

    /// <summary>
    /// Список товаров в стоп-листе
    /// </summary>
    [JsonProperty("products")]
    public List<PluginToServerStopListRemainingMealsStopListProducts> Products { get; set; }
}