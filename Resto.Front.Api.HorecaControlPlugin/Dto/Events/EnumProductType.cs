using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

[JsonConverter(typeof(StringEnumConverter))]
public enum EnumProductType
{
    [EnumMember(Value = "product")] Product,
    [EnumMember(Value = "modifier")] Modifier,
}