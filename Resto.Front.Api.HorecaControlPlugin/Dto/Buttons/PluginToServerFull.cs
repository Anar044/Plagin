using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons
{
    public class PluginToServerFull : APluginToServer
    {
        [JsonProperty("opened")] public DateTime? Opened { get; set; }
        [JsonProperty("closed")] public DateTime? Closed { get; set; }

        [JsonProperty("summaryOfRestaurant")] public PluginToServerSummaryOfRestaurant SummaryOfRestaurant { get; set; }

        [JsonProperty("revenueByWaiters")] public PluginToServerRevenueByWaiters RevenueByWaiters { get; set; }

        [JsonProperty("stopListRemainingMeals")]
        public PluginToServerStopListRemainingMeals StopListRemainingMeals { get; set; }

        [JsonProperty("topTenMealsByRevenue")]
        public PluginToServerTopTenMealsByRevenue TopTenMealsByRevenue { get; set; }

        [JsonProperty("highRiskOperation")] public PluginToServerHighRiskOperation HighRiskOperation { get; set; }

        [JsonProperty("ordersDetails")] public List<PluginToServerOrderDetails> OrdersDetails { get; set; }
    }
}