using LinqToDB.Mapping;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;

[Table(Name = "HighRiskOperations")]
public class HighRiskOperation
{
    [Column(Name = "Id"), PrimaryKey, Identity]
    public int Id { get; set; }

    [Column(Name = "TerminalsGroupId")] public Guid TerminalsGroupId { get; set; }

    [Column(Name = "Date")] public DateTime Date { get; set; }

    [Column(Name = "UserId")] public int UserId { get; set; }

    [Column(Name = "ShiftId")] public int ShiftId { get; set; }

    [Column(Name = "Action")] public string Action { get; set; }

    [Association(ThisKey = "ShiftId", OtherKey = "Id")]
    [Newtonsoft.Json.JsonIgnore]
    public Shift Shift { get; set; }

    [Association(ThisKey = "UserId", OtherKey = "Id")]
    [Newtonsoft.Json.JsonIgnore]
    public User User { get; set; }
}