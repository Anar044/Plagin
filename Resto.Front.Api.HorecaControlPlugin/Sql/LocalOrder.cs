using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Data.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Resto.Front.Api.HorecaControlPlugin.Sql;

public sealed class LocalOrder
{
    public Guid OrderId { get; set; }
    public Guid LastChangeTerminalId { get; set; }
    public bool IsBanquet { get; set; }

    public DateTime OpenTime { get; set; }
    public DateTime? BillTime { get; set; }
    public DateTime? CloseTime { get; set; }

    public decimal ResultSum { get; set; }
    public int Number { get; set; }

    public string ClientName { get; set; }

    public string Phone { get; set; }
    public string Floor { get; set; }

    public List<KeyValueLocalOrderClass> Discounts { get; set; }
    public List<KeyValueLocalOrderClass> Surcharges { get; set; }

    public List<KeyValueLocalOrderClass> Tables { get; set; }


    public Guid WaiterId { get; set; }

    public string WaiterName { get; set; }
    public int ShiftCount { get; set; }

    public int Revision { get; set; }
    public DeliveryStatus? DeliveryStatus { get; set; }
    public bool ToDelete { get; set; }


    public static List<KeyValueLocalOrderClass> GetTables(IOrder order)
    {
        return order.Tables.Select(e =>
            new KeyValueLocalOrderClass()
            {
                Id = e.Id,
                Name = e.Name,
                Value = e.Number
            }).ToList();
    }


    public static (List<KeyValueLocalOrderClass> Discounts, List<KeyValueLocalOrderClass> Surcharges)
        GetDiscountsSurchargesLists(
            IOrder order)
    {
        var surcharges = new List<KeyValueLocalOrderClass>();
        var discounts = new List<KeyValueLocalOrderClass>();

        if (order.AppliedDiscounts != null && order.AppliedDiscounts.Any())
        {
            foreach (var discount in order.AppliedDiscounts)
            {
                var valuePercent = 0M;
                try
                {
                    valuePercent =
                        decimal.Round((discount.DiscountSum / (order.ResultSum + discount.DiscountSum)) * 100M, 2);
                }
                catch (Exception ex)
                {
                }

                if (discount.DiscountSum > 0)
                    discounts.Add(new()
                    {
                        Name = discount.Discount.DiscountType.Name,
                        Value = discount.DiscountSum,
                        ValuePercent = valuePercent,
                        Id = discount.Discount.DiscountType.Id,
                    });
                if (discount.DiscountSum < 0)
                    surcharges.Add(new()
                    {
                        Name = discount.Discount.DiscountType.Name,
                        Value = (-1M * discount.DiscountSum),
                        ValuePercent = -1M * valuePercent,
                        Id = discount.Discount.DiscountType.Id,
                    });
            }
        }

        return (discounts, surcharges);
    }
}

public class KeyValueLocalOrderClass
{
    public string Name { get; set; }

    public decimal? Value { get; set; }

    public decimal? ValuePercent { get; set; }

    public Guid Id { get; set; }
}