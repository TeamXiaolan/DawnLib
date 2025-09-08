using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dawn;
public class NamespacedKeyConverter : JsonConverter
{

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is NamespacedKey key)
        {
            writer.WriteValue(key.ToString());
        }
        else
        {
            writer.WriteNull();
        }
    }
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        string? str = reader.Value?.ToString();
        if (str == null)
            return null;

        NamespacedKey parsed = NamespacedKey.ForceParse(str);

        if (objectType == typeof(NamespacedKey))
        {
            return parsed;
        }

        if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(NamespacedKey<>))
        {
            MethodInfo asTyped = typeof(NamespacedKey).GetMethod(nameof(NamespacedKey.AsTyped))!;
            MethodInfo genericAsTyped = asTyped.MakeGenericMethod(objectType.GetGenericArguments()[0]);
            return genericAsTyped.Invoke(parsed, null);
        }

        throw new JsonSerializationException($"Cannot deserialize {objectType}");
    }
    public override bool CanConvert(Type objectType)
    {
        if (objectType == typeof(NamespacedKey))
            return true;
        if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(NamespacedKey<>))
            return true;
        return false;
    }
}

public class NamespacedKeyDictionaryConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsGenericType &&
            objectType.GetGenericTypeDefinition() == typeof(Dictionary<,>) && // is dictonary
            objectType.GetGenericArguments()[0] == typeof(NamespacedKey); // dictonary key is namespacedkey
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        var dict = (System.Collections.IDictionary)value!;
        foreach (var keyObj in dict.Keys)
        {
            var key = (NamespacedKey)keyObj;
            var val = dict[keyObj];
            writer.WritePropertyName(key.ToString());
            serializer.Serialize(writer, val);
        }

        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var valueType = objectType.GetGenericArguments()[1];
        var dictType = typeof(Dictionary<,>).MakeGenericType(typeof(NamespacedKey), valueType);
        var dict = (System.Collections.IDictionary)Activator.CreateInstance(dictType)!;

        var obj = JObject.Load(reader);
        foreach (var prop in obj.Properties())
        {
            var key = NamespacedKey.ForceParse(prop.Name);
            var val = prop.Value.ToObject(valueType, serializer);
            dict[key] = val!;
        }

        return dict;
    }
}