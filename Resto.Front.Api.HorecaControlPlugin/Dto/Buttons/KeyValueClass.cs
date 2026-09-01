using System;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

/// <summary>
/// Вспомогательный класс для хранения ключевых параметров скидок, надбавок, оплат, чаевых
/// </summary>
public class KeyValueClass
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("value")]
    public decimal Value { get; set; }


    [JsonProperty("type")]
    public EnumPaymentType? Type { get; set; }

    [JsonIgnore] public Guid Id { get; set; }
}