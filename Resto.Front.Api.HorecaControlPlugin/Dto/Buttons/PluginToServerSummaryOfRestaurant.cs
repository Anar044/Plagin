using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Abstracts;
using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

/// <summary>
/// ����� ���������� ������ 
/// </summary>
public class PluginToServerSummaryOfRestaurant : APluginToServer<PluginToServerSummaryOfRestaurantTerminalsGroup,
    PluginToServerSummaryOfRestaurantTerminalsGroupRestaurantSection>, ITableAndRevenueReportData
{
    /// <summary>
    /// ���������� �������� �����������
    /// </summary>
    [JsonProperty("activeEmployees")]
    public int ActiveEmployees { get; set; }

    [JsonProperty("terminalsGroups")]
    public sealed override List<PluginToServerSummaryOfRestaurantTerminalsGroup> TerminalsGroups { get; set; }

    /// <summary>
    /// ���������� �������� �������
    /// </summary>
    [JsonProperty("openOrders")]
    public int OpenedOrders { get; set; }

    /// <summary>
    /// ���������� �������� �������
    /// </summary>
    [JsonProperty("closedOrders")]
    public int ClosedOrders { get; set; }

    /// <summary>
    /// ��������� ����� �� �������� � ���������� �������
    /// </summary>
    [JsonProperty("expectedRevenueMoneySum")]
    public decimal ExpectedRevenueMoneySum { get; set; }

    /// <summary>
    /// ��������� ����� �� �������� �������
    /// </summary>
    [JsonProperty("openOrdersMoneySum")]
    public decimal OpenOrdersMoneySum { get; set; }

    /// <summary>
    /// ����� �� �������� �������
    /// </summary>
    [JsonProperty("closedOrdersMoneySum")]
    public decimal ClosedOrdersMoneySum { get; set; }

    /// <summary>
    /// �������� ������ 
    /// </summary>
    [JsonProperty("activeTables")]
    public int ActiveTables { get; set; }

    /// <summary>
    /// ���������� ������
    /// </summary>
    [JsonProperty("inActiveTables")]
    public int InActiveTables { get; set; }

    /// <summary>
    /// ���������� �������� �������
    /// </summary>
    [JsonProperty("banquetsToday")]
    public int BanquetsToday { get; set; }

    /// <summary>
    /// ���������� �������� � ������
    /// </summary>
    [JsonProperty("banquetsWeek")]
    public int BanquetsWeek { get; set; }

    /// <summary>
    /// ���������� �������� � �����
    /// </summary>
    [JsonProperty("banquetsMonth")]
    public int BanquetsMonth { get; set; }

    /// <summary>
    /// �������� ������ � %%
    /// </summary>
    [JsonProperty("workLoad")]
    public decimal WorkLoad { get; set; }

    /// <summary>
    /// ���������� ������
    /// </summary>
    [JsonProperty("numberOfGuest")]
    public int NumberOfGuest { get; set; }

    /// <summary>
    /// ���������� �������
    /// </summary>
    [JsonProperty("orderCount")]
    public int OrderCount { get; set; }

    /// <summary>
    /// ������
    /// </summary>
    [JsonProperty("discounts")]
    public List<KeyValueClass> Discounts { get; set; }

    /// <summary>
    /// ��������
    /// </summary>
    [JsonProperty("surcharges")]
    public List<KeyValueClass> Surcharges { get; set; }

    /// <summary>
    /// �������
    /// </summary>
    [JsonProperty("payments")]
    public List<KeyValueClass> Payments { get; set; }

    /// <summary>
    /// ������
    /// </summary>
    [JsonProperty("tips")]
    public List<KeyValueClass> Tips { get; set; }

    /// <summary>
    /// ���������� �������� ��������
    /// </summary>
    [JsonProperty("openedBanquetOrders")]
    public int OpenedBanquetOrders { get; set; }

    /// <summary>
    /// ����� � �������� ��������
    /// </summary>
    [JsonProperty("openBanquetOrdersMoneySum")]
    public decimal OpenBanquetOrdersMoneySum { get; set; }

    /// <summary>
    /// �������� ����� �� �������� � ���������� ��������
    /// </summary>
    [JsonProperty("expectedBanquetRevenueMoneySum")]
    public decimal ExpectedBanquetRevenueMoneySum { get; set; }

    /// <summary>
    /// ���������� �������������� ������ �  ��������
    /// </summary>
    [JsonProperty("activeBanquetTables")]
    public int ActiveBanquetTables { get; set; }

    /// <summary>
    /// ���������� ������ � ��������
    /// </summary>
    [JsonProperty("banquetNumberOfGuest")]
    public int BanquetNumberOfGuest { get; set; }

    /// <summary>
    /// ���������� �������� ��������
    /// </summary>
    [JsonProperty("closedBanquetOrders")]
    public int ClosedBanquetOrders { get; set; }

    /// <summary>
    /// ����� �������� ��������
    /// </summary>
    [JsonProperty("closedBanquetOrdersMoneySum")]
    public decimal ClosedBanquetOrdersMoneySum { get; set; }

    public PluginToServerSummaryOfRestaurant()
    {
        Discounts = new List<KeyValueClass>();
        Surcharges = new List<KeyValueClass>();
        Payments = new List<KeyValueClass>();
        Tips = new List<KeyValueClass>();

        TerminalsGroups =
            base.GenerateTerminalsGroups<PluginToServerSummaryOfRestaurantTerminalsGroup,
                PluginToServerSummaryOfRestaurantTerminalsGroupRestaurantSection>();
        TotalTables = TerminalsGroups?.Sum(x => x.TotalTables) ?? 0;
    }

    public void CalculateFloorTotal()
    {
        ExpectedRevenueMoneySum = TerminalsGroups.Sum(x => x.ExpectedRevenueMoneySum);
        OpenOrdersMoneySum = TerminalsGroups.Sum(x => x.OpenOrdersMoneySum);
        ClosedOrdersMoneySum = TerminalsGroups.Sum(x => x.ClosedOrdersMoneySum);
        ActiveTables = TerminalsGroups.Sum(x => x.ActiveTables);
        TotalTables = TerminalsGroups.Sum(x => x.TotalTables);
        InActiveTables = TotalTables - ActiveTables;
        WorkLoad = TotalTables > 0 ? decimal.Round(ActiveTables / (decimal)TotalTables * 100, 2) : 0;
        NumberOfGuest = TerminalsGroups.Sum(x => x.NumberOfGuest);
        OrderCount = TerminalsGroups.Sum(x => x.OrderCount);
        BanquetsToday = TerminalsGroups.Sum(x => x.BanquetsToday);
        BanquetsWeek = TerminalsGroups.Sum(x => x.BanquetsWeek);
        BanquetsMonth = TerminalsGroups.Sum(x => x.BanquetsMonth);
        OpenedOrders = TerminalsGroups.Sum(x => x.OpenedOrders);
        ClosedOrders = TerminalsGroups.Sum(x => x.ClosedOrders);

        OpenedBanquetOrders = TerminalsGroups.Sum(x => x.OpenedBanquetOrders);
        OpenBanquetOrdersMoneySum = TerminalsGroups.Sum(x => x.OpenBanquetOrdersMoneySum);
        ExpectedBanquetRevenueMoneySum = TerminalsGroups.Sum(x => x.ExpectedBanquetRevenueMoneySum);
        ActiveBanquetTables = TerminalsGroups.Sum(x => x.ActiveBanquetTables);
        BanquetNumberOfGuest = TerminalsGroups.Sum(x => x.BanquetNumberOfGuest);
        ClosedBanquetOrders = TerminalsGroups.Sum(x => x.ClosedBanquetOrders);
        ClosedBanquetOrdersMoneySum = TerminalsGroups.Sum(x => x.ClosedBanquetOrdersMoneySum);

        // MOCK
        ExpectedRevenueMoneySum += ExpectedBanquetRevenueMoneySum;
        OpenOrdersMoneySum += OpenBanquetOrdersMoneySum;
        ClosedOrdersMoneySum += ClosedBanquetOrdersMoneySum;


        CalculateDiscountSurchargePayment();
    }


    public void CalculateTotal()
    {
        ExpectedRevenueMoneySum = TerminalsGroups.Sum(x => x.ExpectedRevenueMoneySum);
        OpenOrdersMoneySum = TerminalsGroups.Sum(x => x.OpenOrdersMoneySum);
        ClosedOrdersMoneySum = TerminalsGroups.Sum(x => x.ClosedOrdersMoneySum);
        ActiveTables = TerminalsGroups.Sum(x => x.ActiveTables);
        TotalTables = TerminalsGroups.Sum(x => x.TotalTables);
        InActiveTables = TotalTables - ActiveTables;
        WorkLoad = TotalTables > 0 ? decimal.Round(ActiveTables / (decimal)TotalTables * 100, 2) : 0;
        NumberOfGuest = TerminalsGroups.Sum(x => x.NumberOfGuest);
        OrderCount = TerminalsGroups.Sum(x => x.OrderCount);
        BanquetsToday = TerminalsGroups.Sum(x => x.BanquetsToday);
        BanquetsWeek = TerminalsGroups.Sum(x => x.BanquetsWeek);
        BanquetsMonth = TerminalsGroups.Sum(x => x.BanquetsMonth);
        OpenedOrders = TerminalsGroups.Sum(x => x.OpenedOrders);
        ClosedOrders = TerminalsGroups.Sum(x => x.ClosedOrders);


        Discounts = Discounts.OrderBy(x => x.Name).ToList();
        Surcharges = Surcharges.OrderBy(x => x.Name).ToList();
        Payments = Payments.OrderBy(x => x.Name).ToList();
        CalculateDiscountSurchargePayment();

        TerminalsGroups.RemoveAll(x => x.OrderCount == 0);
    }

    private void CalculateDiscountSurchargePayment()
    {
        foreach (var rs in TerminalsGroups)
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
                        Type = EnumPaymentType.Other,
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
                        Type = EnumPaymentType.Other,
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