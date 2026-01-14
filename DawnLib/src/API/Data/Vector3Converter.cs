using System;
using Newtonsoft.Json;
using UnityEngine;

public sealed class Vector3Converter : JsonConverter<Vector3>
{
    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x"); writer.WriteValue(value.x);
        writer.WritePropertyName("y"); writer.WriteValue(value.y);
        writer.WritePropertyName("z"); writer.WriteValue(value.z);
        writer.WriteEndObject();
    }

    public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        float x = 0, y = 0, z = 0;
        if (reader.TokenType != JsonToken.StartObject) throw new JsonSerializationException("Expected StartObject for Vector3");
        while (reader.Read() && reader.TokenType != JsonToken.EndObject)
        {
            if (reader.TokenType != JsonToken.PropertyName) continue;
            var name = (string)reader.Value!;
            reader.Read();
            switch (name)
            {
                case "x": x = Convert.ToSingle(reader.Value); break;
                case "y": y = Convert.ToSingle(reader.Value); break;
                case "z": z = Convert.ToSingle(reader.Value); break;
            }
        }
        return new Vector3(x, y, z);
    }
}
