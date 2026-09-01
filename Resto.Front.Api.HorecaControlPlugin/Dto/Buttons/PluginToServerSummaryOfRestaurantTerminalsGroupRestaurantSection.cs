using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Abstracts;
using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

/// <summary>
/// ����� ���������� ������ �� �������
/// </summary>
public class
    PluginToServerSummaryOfRestaurantTerminalsGroupRestaurantSection : APluginToServerTerminalsGroupRestaurantSection,
    ITableAndRevenueReportData
{
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

    public PluginToServerSummaryOfRestaurantTerminalsGroupRestaurantSection()
    {
        Discounts = new List<KeyValueClass>();
        Surcharges = new List<KeyValueClass>();
        Payments = new List<KeyValueClass>();
        Tips = new List<KeyValueClass>();
    }

    public void CalculationAllDiscountsDonationsPayments(IOrder order)
    {
#if DEBUG
        PluginContext.Log.Debug(
            $"CalculationAllDiscountsDonationsPayments:: {order.Number} : {order.ResultSum} started");
#endif
        if (!Properties.Settings.Default.DisableDiscountCalc)
        {
            if (order.AppliedDiscounts is not null && order.AppliedDiscounts.Any())
            {
                foreach (var item in order.AppliedDiscounts.ToList())
                {
                    // Discount
                    if (item.DiscountSum >= 0)
                    {
                        var existDiscount =
                            Discounts.FirstOrDefault(x =>
                                x.Id == item.Discount.DiscountType.Id);
                        if (existDiscount is null)
                        {
                            existDiscount = new KeyValueClass
                            {
                                Name = item.Discount.DiscountType.Name,
                                Value = 0,
                                Type = EnumPaymentType.Other,
                                Id = item.Discount.DiscountType.Id
                            };
                            Discounts.Add(existDiscount);
                        }

                        existDiscount.Value += item.DiscountSum;
                    }
                    else
                    {
                        var existSurcharge =
                            Surcharges.FirstOrDefault(x =>
                                x.Id == item.Discount.DiscountType.Id);
                        if (existSurcharge is null)
                        {
                            existSurcharge = new KeyValueClass
                            {
                                Name = item.Discount.DiscountType.Name,
                                Value = 0,
                                Type = EnumPaymentType.Other,
                                Id = item.Discount.DiscountType.Id
                            };
                            Surcharges.Add(existSurcharge);
                        }

                        existSurcharge.Value += (-1 * item.DiscountSum);
                    }
                }
            }
        }

        if (order.Donations is not null && order.Donations.Any())
        {
            foreach (var donation in order.Donations)
            {
                var existTip =
                    Tips.FirstOrDefault(x =>
                        x.Id == donation.Type.Id);
                var enumType = EnumPaymentType.Other;


                if (existTip is null)
                {
                    if (donation.Type.PrintCheque &&
                        donation.Type.Kind == PaymentTypeKind.Cash)
                    {
                        enumType = EnumPaymentType.Cash;
                    }
                    else if (donation.Type.PrintCheque)
                        enumType = EnumPaymentType.Card;

                    existTip = new KeyValueClass
                    {
                        Name = donation.Type.Name,
                        Value = 0,
                        Type = enumType,
                        Id = donation.Type.Id
                    };
                    Tips.Add(existTip);
                }

                existTip.Value += donation.Sum;
            }
        }

        if (order.Payments is not null && order.Payments.Any())
        {
#if DEBUG
            PluginContext.Log.Debug($"CalculationAllDiscountsDonationsPayments:: {new string('=', 80)}");
            PluginContext.Log.Debug(
                $"CalculationAllDiscountsDonationsPayments:: {order.Number} : {order.ResultSum} payments. All payments");
            PluginContext.Log.Debug(order.Payments.ToJson());

            PluginContext.Log.Debug(
                $"CalculationAllDiscountsDonationsPayments:: {order.Number} : {order.ResultSum} payments. Extracted payments");
            PluginContext.Log.Debug(order.Payments.Where(x =>
                //x.Type.PrintCheque
                // && 
                x.Type.DiscountType == null
                && x.Status == PaymentStatus.Processed
            ).ToList().ToJson());
            PluginContext.Log.Debug($"CalculationAllDiscountsDonationsPayments:: {new string('=', 80)}");
#endif

            foreach (var payment in order.Payments.Where(x =>
                         //x.Type.PrintCheque
                         // && 
                         x.Type.DiscountType == null
                         && x.Status == PaymentStatus.Processed
                     ).ToList())
            {
                if (PluginHelpers.ExcludedPayments.Contains(payment.Type.Id))
                    continue;

                var enumType = EnumPaymentType.Other;
                var sum = payment.Sum;


                if (payment.Type.PrintCheque &&
                    payment.Type.Kind == PaymentTypeKind.Cash)
                {
                    var totalPaySum = order.Payments.Where(x => x.Status == PaymentStatus.Processed).ToList()
                        ?.Sum(x => x.Sum) ?? 0M;
                    var difference = totalPaySum - order.ResultSum;
#if DEBUG
                    PluginContext.Log.Debug(
                        $"CalculationAllDiscountsDonationsPayments (Cash) :: {totalPaySum} {order.ResultSum} {difference}");
#endif
                    enumType = EnumPaymentType.Cash;
                    sum = payment.Sum - difference;
                }
                else if (payment.Type.PrintCheque)
                    enumType = EnumPaymentType.Card;

                var existPayment =
                    Payments.FirstOrDefault(x =>
                        x.Id == payment.Type.Id);

                if (existPayment is null)
                {
                    existPayment = new KeyValueClass
                    {
                        Name = payment.Type.Name,
                        Value = 0,
                        Type = enumType,
                        Id = payment.Type.Id
                    };
                    Payments.Add(existPayment);
                }

                existPayment.Value += sum;
            }
#if DEBUG
            PluginContext.Log.Debug(
                $"CalculationAllDiscountsDonationsPayments:: {order.Number} : {order.ResultSum} calc total payments");
            PluginContext.Log.Debug(Payments.ToJson());
#endif
        }
#if DEBUG
        PluginContext.Log.Debug(
            $"CalculationAllDiscountsDonationsPayments:: {order.Number} : {order.ResultSum} finished");
#endif
    }
}