using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

public class PluginToServerTopTenMealsByRevenue : APluginToServer
{
    [JsonProperty("terminalsGroup")] public string TerminalsGroup { get; set; } = PluginHelpers.GroupName.Name;

    [JsonProperty("products")] public List<PluginToServerTopTenMealsByRevenueProduct> Products { get; set; }

    public PluginToServerTopTenMealsByRevenue()
    {
        Products = new List<PluginToServerTopTenMealsByRevenueProduct>();
    }
}

public class PluginToServerTopTenMealsByRevenueProduct
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("revenue")]
    public decimal Revenue { get; set; }

    [JsonProperty("count")]
    public decimal Count { get; set; }
}