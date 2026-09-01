using LinqToDB.Mapping;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;

[Table(Name = "Users")]
public class User
{
    [Column(Name = "Id"), PrimaryKey, Identity]
    public int Id { get; set; }

    [Column(Name = "UserId")] public Guid UserId { get; set; }

    [Column(Name = "UserName")] public string UserName { get; set; }
}