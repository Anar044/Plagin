using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Abstracts;
using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

/// <summary>
/// ����� ���������� ������ �� ������� ����������
/// </summary>
public class PluginToServerSummaryOfRestaurantTerminalsGroup :
    APluginToServerTerminalsGroup<PluginToServerSummaryOfRestaurantTerminalsGroupRestaurantSection>,
    ITableAndRevenueReportData
{
    [JsonProperty("restaurantSections")]
    public sealed override List<PluginToServerSummaryOfRestaurantTerminalsGroupRestaurantSection> RestaurantSections
    {
        get;
        set;
    }


    [JsonProperty("openOrders")]
    public int OpenedOrders { get; set; }
    [JsonProperty("closedOrders")]
    public int ClosedOrders { get; set; }
    [JsonProperty("expectedRevenueMoneySum")]
    public decimal ExpectedRevenueMoneySum { get; set; }
    [JsonProperty("openOrdersMoneySum")]
    public decimal OpenOrdersMoneySum { get; set; }
    [JsonProperty("closedOrdersMoneySum")]
    public decimal ClosedOrdersMoneySum { get; set; }
    [JsonProperty("activeTables")]
    public int ActiveTables { get; set; }
    [JsonProperty("inActiveTables")]
    public int InActiveTables { get; set; }
    [JsonProperty("banquetsToday")]
    public int BanquetsToday { get; set; }
    [JsonProperty("banquetsWeek")]
    public int BanquetsWeek { get; set; }
    [JsonProperty("banquetsMonth")]
    public int BanquetsMonth { get; set; }
    [JsonProperty("workLoad")]
    public decimal WorkLoad { get; set; }
    [JsonProperty("numberOfGuest")]
    public int NumberOfGuest { get; set; }
    [JsonProperty("orderCount")]
    public int OrderCount { get; set; }
    [JsonProperty("discounts")]
    public List<KeyValueClass> Discounts { get; set; }
    [JsonProperty("surcharges")]
    public List<KeyValueClass> Surcharges { get; set; }
    [JsonProperty("payments")]
    public List<KeyValueClass> Payments { get; set; }
    [JsonProperty("tips")]
    public List<KeyValueClass> Tips { get; set; }
    [JsonProperty("openedBanquetOrders")]
    public int OpenedBanquetOrders { get; set; }
    [JsonProperty("openBanquetOrdersMoneySum")]
    public decimal OpenBanquetOrdersMoneySum { get; set; }
    [JsonProperty("expectedBanquetRevenueMoneySum")]
    public decimal ExpectedBanquetRevenueMoneySum { get; set; }
    [JsonProperty("activeBanquetTables")]
    public int ActiveBanquetTables { get; set; }
    [JsonProperty("banquetNumberOfGuest")]
    public int BanquetNumberOfGuest { get; set; }
    [JsonProperty("closedBanquetOrders")]
    public int ClosedBanquetOrders { get; set; }
    [JsonProperty("closedBanquetOrdersMoneySum")]
    public decimal ClosedBanquetOrdersMoneySum { get; set; }

    public PluginToServerSummaryOfRestaurantTerminalsGroup()
    {
        Discounts = new List<KeyValueClass>();
        Surcharges = new List<KeyValueClass>();
        Payments = new List<KeyValueClass>();
        Tips = new List<KeyValueClass>();
    }

    public void CalculateTotalGroup()
    {
        ExpectedRevenueMoneySum = RestaurantSections.Sum(x => x.ExpectedRevenueMoneySum);
        OpenOrdersMoneySum = RestaurantSections.Sum(x => x.OpenOrdersMoneySum);
        ClosedOrdersMoneySum = RestaurantSections.Sum(x => x.ClosedOrdersMoneySum);
        OpenedOrders = RestaurantSections.Sum(x => x.OpenedOrders);
        ClosedOrders = RestaurantSections.Sum(x => x.ClosedOrders);
        ActiveTables = RestaurantSections.Sum(x => x.ActiveTables);
        TotalTables = RestaurantSections.Sum(x => x.TotalTables);
        InActiveTables = TotalTables - ActiveTables;
        WorkLoad = TotalTables > 0 ? decimal.Round(ActiveTables / (decimal)TotalTables * 100, 2) : 0;
        NumberOfGuest = RestaurantSections.Sum(x => x.NumberOfGuest);
        OrderCount = RestaurantSections.Sum(x => x.OrderCount);
        BanquetsToday = RestaurantSections.Sum(x => x.BanquetsToday);
        BanquetsWeek = RestaurantSections.Sum(x => x.BanquetsWeek);
        BanquetsMonth = RestaurantSections.Sum(x => x.BanquetsMonth);


        Discounts = Discounts.OrderBy(x => x.Name).ToList();
        Surcharges = Surcharges.OrderBy(x => x.Name).ToList();
        Payments = Payments.OrderBy(x => x.Name).ToList();
        Tips = Tips.OrderBy(x => x.Name).ToList();
        CalculateDiscountSurchargePayment();
    }

    public void CalculateFloorTotalGroup()
    {
        ExpectedRevenueMoneySum = RestaurantSections.Sum(x => x.ExpectedRevenueMoneySum);
        OpenOrdersMoneySum = RestaurantSections.Sum(x => x.OpenOrdersMoneySum);
        ClosedOrdersMoneySum = RestaurantSections.Sum(x => x.ClosedOrdersMoneySum);
        OpenedOrders = RestaurantSections.Sum(x => x.OpenedOrders);
        ClosedOrders = RestaurantSections.Sum(x => x.ClosedOrders);

        OpenedBanquetOrders = RestaurantSections.Sum(x => x.OpenedBanquetOrders);
        OpenBanquetOrdersMoneySum = RestaurantSections.Sum(x => x.OpenBanquetOrdersMoneySum);
        ExpectedBanquetRevenueMoneySum = RestaurantSections.Sum(x => x.ExpectedBanquetRevenueMoneySum);
        ActiveBanquetTables = RestaurantSections.Sum(x => x.ActiveBanquetTables);
        BanquetNumberOfGuest = RestaurantSections.Sum(x => x.BanquetNumberOfGuest);
        ClosedBanquetOrders = RestaurantSections.Sum(x => x.ClosedBanquetOrders);
        ClosedBanquetOrdersMoneySum = RestaurantSections.Sum(x => x.ClosedBanquetOrdersMoneySum);


        ActiveTables = RestaurantSections.Sum(x => x.ActiveTables);
        TotalTables = RestaurantSections.Sum(x => x.TotalTables);
        InActiveTables = TotalTables - ActiveTables;
        WorkLoad = TotalTables > 0 ? decimal.Round(ActiveTables / (decimal)TotalTables * 100, 2) : 0;
        NumberOfGuest = RestaurantSections.Sum(x => x.NumberOfGuest);
        OrderCount = RestaurantSections.Sum(x => x.OrderCount);
        BanquetsToday = RestaurantSections.Sum(x => x.BanquetsToday);
        BanquetsWeek = RestaurantSections.Sum(x => x.BanquetsWeek);
        BanquetsMonth = RestaurantSections.Sum(x => x.BanquetsMonth);


        Discounts = Discounts.OrderBy(x => x.Name).ToList();
        Surcharges = Surcharges.OrderBy(x => x.Name).ToList();
        Payments = Payments.OrderBy(x => x.Name).ToList();
        Tips = Tips.OrderBy(x => x.Name).ToList();

        // MOCK
        ExpectedRevenueMoneySum += ExpectedBanquetRevenueMoneySum;
        OpenOrdersMoneySum += OpenBanquetOrdersMoneySum;
        ClosedOrdersMoneySum += ClosedBanquetOrdersMoneySum;

        CalculateDiscountSurchargePayment();
        RestaurantSections.RemoveAll(x => x.OrderCount == 0);
    }

    private void CalculateDiscountSurchargePayment()
    {
        foreach (var rs in RestaurantSections)
        {
            rs.Discounts.ForEach(discountRs =>
            {
                var existDiscount = Discounts.FirstOrDefault(x => x.Id == discountRs.Id);
                if (existDiscount is null)
                {
                    existDiscount = new KeyValueClass
                    {
                        Name = discountRs.Name,
                        Value = 0,
                        Type = discountRs.Type,
                        Id = discountRs.Id,
                    };
                    Discounts.Add(existDiscount);
                }

                existDiscount.Value += discountRs.Value;
            });
            rs.Surcharges.ForEach(surchargeRs =>
            {
                var existSurchage = Surcharges.FirstOrDefault(x => x.Id == surchargeRs.Id);
                if (existSurchage is null)
                {
                    existSurchage = new KeyValueClass
                    {
                        Name = surchargeRs.Name,
                        Value = 0,
                        Type = surchargeRs.Type,
                        Id = surchargeRs.Id,
                    };
                    Surcharges.Add(existSurchage);
                }

                existSurchage.Value += surchargeRs.Value;
            });
            rs.Payments.ForEach(paymentRs =>
            {
                var existPayment = Payments.FirstOrDefault(x => x.Id == paymentRs.Id);
                if (existPayment is null)
                {
                    existPayment = new KeyValueClass
                    {
                        Name = paymentRs.Name,
                        Value = 0,
                        Type = paymentRs.Type,
                        Id = paymentRs.Id,
                    };
                    Payments.Add(existPayment);
                }

                existPayment.Value += paymentRs.Value;
            });

            rs.Tips.ForEach(tipRs =>
            {
                var existTip = Tips.FirstOrDefault(x => x.Id == tipRs.Id);
                if (existTip is null)
                {
                    existTip = new KeyValueClass
                    {
                        Name = tipRs.Name,
                        Value = 0,
                        Type = tipRs.Type,
                        Id = tipRs.Id,
                    };
                    Tips.Add(existTip);
                }

                existTip.Value += tipRs.Value;
            });
        }
    }
}