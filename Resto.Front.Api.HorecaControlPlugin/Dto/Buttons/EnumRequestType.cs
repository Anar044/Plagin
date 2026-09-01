using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

[JsonConverter(typeof(StringEnumConverter))]
public enum EnumRequestType
{
    [EnumMember(Value = "summaryOfRestaurant")]
    SummaryOfRestaurant,

    [EnumMember(Value = "revenueByRestaurantsRegistersFloors")]
    RevenueByRestaurantsRegistersFloors,

    [EnumMember(Value = "revenueByWaiters")]
    RevenueByWaiters,

    [EnumMember(Value = "currentShiftOrdersList")]
    CurrentShiftOrdersList,

    [EnumMember(Value = "topTenMealsByRevenue")]
    TopTenMealsByRevenue,

    [EnumMember(Value = "stopListRemainingMeals")]
    StopListRemainingMeals,

    [EnumMember(Value = "tablesWithOpenOrders")]
    TablesWithOpenOrders,

    [EnumMember(Value = "highRiskOperations")]
    HighRiskOperations,

    [EnumMember(Value = "order")] Order,

    [EnumMember(Value = "getFullDataReport")]
    GetFullReport,
}