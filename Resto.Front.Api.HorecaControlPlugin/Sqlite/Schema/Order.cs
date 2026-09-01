using LinqToDB.Mapping;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;

[Table(Name = "Orders")]
public class Order
{
    [Column(Name = "Id"), PrimaryKey, Identity]
    public int Id { get; set; }

    [Column(Name = "Shiftid")] public int ShiftId { get; set; }

    [Column(Name = "OrderId")] public Guid OrderId { get; set; }

    [Column(Name = "DateTime")] public DateTime? DateTime { get; set; }

    [Column(Name = "Data")] public string Data { get; set; }

    [Column(Name = "Count")] public int Count { get; set; }

    // [Column(Name = "DeliveryStatus")] public DeliveryStatus? DeliveryStatus { get; set; }
    //
    // [Column(Name = "Status")] public OrderStatus? Status { get; set; }
    [Column(Name = "Deleted")] public bool Deleted { get; set; }


    [Association(ThisKey = "ShiftId", OtherKey = "Id")]
    [Newtonsoft.Json.JsonIgnore]
    public Shift Shift { get; set; }
}