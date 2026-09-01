using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

/// <summary>
/// Столы с открытыми заказами
/// </summary>
public class PluginToServerTablesWithOpenOrders : APluginToServer
{
    /// <summary>
    /// Название группы терминалов
    /// </summary>
    [JsonProperty("terminalsGroup")]
    public string TerminalsGroup { get; set; } = PluginHelpers.GroupName.Name;

    /// <summary>
    /// Список секций с открытыми заказами
    /// </summary>
    [JsonProperty("restaurantSections")]
    public List<PluginToServerTablesWithOpenOrdersRestaurantSections> RestaurantSections { get; set; } = new();
}

/// <summary>
/// Секция с открытыми заказами на столах
/// </summary>
public class PluginToServerTablesWithOpenOrdersRestaurantSections
{
    /// <summary>
    /// Идентификатор секции
    /// </summary>
    [JsonProperty("restaurantSectionId")]
    public Guid RestaurantSectionId { get; set; }

    /// <summary>
    /// Название секции
    /// </summary>
    [JsonProperty("restaurantSectionName")]
    public string RestaurantSectionName { get; set; }

    /// <summary>
    /// Список таблиц с открытыми заказами
    /// </summary>
    [JsonProperty("tables")]
    public List<PluginToServerTablesWithOpenOrdersRestaurantSectionsTable> Tables { get; set; } = new();
}

/// <summary>
/// Таблица с открытыми заказами на столе
/// </summary>
public class PluginToServerTablesWithOpenOrdersRestaurantSectionsTable
{
    /// <summary>
    /// Номер стола
    /// </summary>
    [JsonProperty("tableNumber")]
    public int TableNumber { get; set; }

    /// <summary>
    /// Списко заказов
    /// </summary>
    [JsonProperty("orderNums")]
    public List<int> OrderNums { get; set; }
}