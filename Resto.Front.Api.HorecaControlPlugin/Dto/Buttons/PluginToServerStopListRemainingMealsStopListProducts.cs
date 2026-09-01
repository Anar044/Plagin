using System;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

/// <summary>
/// Класс, описывающий список товаров, которые остались в стоп-листе
/// </summary>
public class PluginToServerStopListRemainingMealsStopListProducts
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Цена
    /// </summary>
    [JsonProperty("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Количество в стоп-листе
    /// </summary>
    [JsonProperty("amount")]
    public decimal Amount { get; set; }
}