using LinqToDB.Mapping;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;

[Table(Name = "Events")]
public class Event
{
    [Column(Name = "Id"), PrimaryKey, Identity]
    public int Id { get; set; }

    [Column(Name = "Uuid")] public Guid Uuid { get; set; }

    [Column(Name = "DateTime")] public DateTime DateTime { get; set; }

    [Column(Name = "Event")] public string EventData { get; set; } // Image type maps to byte array

    [Column(Name = "IsSent")] public bool IsSent { get; set; }
}