using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Converters;

public class PluginToServerEventConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value != null)
        {
            // Без serializer: иначе JsonConverter на типе/свойстве уйдёт в рекурсию.
            var valueToken = JToken.FromObject(value);
            if (valueToken is JObject obj)
            {
                obj["$type"] = value.GetType().FullName;
            }

            valueToken.WriteTo(writer);
            return;
        }

        writer.WriteNull();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var token = JObject.Load(reader);

        var jPropertyType = token?.Properties()?.FirstOrDefault(n => n.Name == "$type")
                            ?? throw new JsonSerializationException("Не удалось определить тип объекта $type");

        var jPropertyValue = jPropertyType?.Value?.ToString();
        if (string.IsNullOrEmpty(jPropertyValue))
            throw new JsonSerializationException("Не удалось определить значение объекта $type");

        var origType = Type.GetType(jPropertyValue)
                       ?? throw new JsonSerializationException("Не удалось преобразовать тип объекта $type");

        var result = token?.ToObject(origType, serializer) ??
                     throw new JsonSerializationException("Не удалось преобразовать объект");

        return result;
    }

    public override bool CanConvert(Type objectType)
    {
        return true;
    }
}
