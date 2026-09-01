using LinqToDB.Mapping;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;

[Table(Name = "Messages")]
public class Message
{
    [Column(Name = "Id"), PrimaryKey, Identity]
    public int Id { get; set; }

    [Column(Name = "Uuid")] public Guid Uuid { get; set; }

    [Column(Name = "DateTime")] public DateTime DateTime { get; set; }

    [Column(Name = "Message")] public string MessageData { get; set; } // Image type maps to byte array

    [Column(Name = "IsSent")] public bool IsSent { get; set; }
}