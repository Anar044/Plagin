using LinqToDB.Mapping;
using System;
using System.Collections.Generic;

namespace Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;

[Table(Name = "Shifts")]
public class Shift
{
    [Column(Name = "Id"), PrimaryKey, Identity]
    public int Id { get; set; }

    [Column(Name = "OpenTime")] public DateTime OpenTime { get; set; }

    [Column(Name = "CloseTime")] public DateTime? CloseTime { get; set; }

    [Column(Name = "OpenerUserId")] public int? OpenerUserId { get; set; }

    [Column(Name = "CloserUserId")] public int? CloserUserId { get; set; }

    [Association(ThisKey = "OpenerUserId", OtherKey = "Id")]
    [Newtonsoft.Json.JsonIgnore]
    public User OpenerUser { get; set; }

    [Association(ThisKey = "CloserUserId", OtherKey = "Id")]
    [Newtonsoft.Json.JsonIgnore]
    public User CloserUser { get; set; }

    // Обратная связь между Shift и HighRiskOperation
    [Association(ThisKey = "Id", OtherKey = "ShiftId")]
    [Newtonsoft.Json.JsonIgnore]
    public IEnumerable<HighRiskOperation> HighRiskOperations { get; set; }

    // Обратная связь между Shift и HighRiskOperation
    [Association(ThisKey = "Id", OtherKey = "ShiftId")]
    [Newtonsoft.Json.JsonIgnore]
    public IEnumerable<Order> Orders { get; set; }
}