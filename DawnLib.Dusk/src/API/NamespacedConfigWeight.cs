using System;
using System.Collections.Generic;
using System.Globalization;
using Dawn;
using Dawn.Utils;
using UnityEngine;

namespace Dusk;
[Serializable]
public class NamespacedConfigWeight
{
    [field: SerializeField]
    [field: InspectorName("Namespace")]
    [field: UnlockedNamespacedKey]
    public NamespacedKey NamespacedKey;

    [field: SerializeField]
    public MathOperation MathOperation = MathOperation.Additive;

    [field: SerializeField]
    [field: Range(-9999, 9999)]
    public float Weight = 0;

    internal static string ConvertToString(NamespacedConfigWeight input)
    {
        // End Result: {NamespacedKey}={Operation}{Weight}
        if (input == null || input.NamespacedKey == null || string.IsNullOrEmpty(input.NamespacedKey.Namespace) || string.IsNullOrEmpty(input.NamespacedKey.Key))
        {
            DuskPlugin.Logger.LogWarning($"Invalid Conversion from NamespacedConfigWeight to string: {input}");
            return string.Empty;
        }

        string Operation;
        Operation = input.MathOperation switch
        {
            MathOperation.Additive => "+",
            MathOperation.Subtractive => "-",
            MathOperation.Multiplicative => "*",
            MathOperation.Divisive => "/",
            _ => "+",
        };
        string result = $"{input.NamespacedKey}={Operation}{input.Weight}";
        return result;
    }

    internal static string ConvertManyToString(List<NamespacedConfigWeight> input)
    {
        string result = string.Empty;
        foreach (NamespacedConfigWeight item in input)
        {
            result += $"{ConvertToString(item)},";
        }

        result = result.RemoveEnd(",");
        return result;
    }

    internal static NamespacedConfigWeight ConvertFromString(string input)
    {
        string[] parts = input.Split('=');
        NamespacedKey namespacedKey = NamespacedKey.ForceParse(parts[0]);
        MathOperation operation = MathOperation.Additive;
        float weight = 0;
        if (parts.Length > 1)
        {
            operation = parts[1][0] switch
            {
                '+' => MathOperation.Additive,
                '-' => MathOperation.Subtractive,
                '*' => MathOperation.Multiplicative,
                '/' => MathOperation.Divisive,
                _ => MathOperation.Additive,
            };
            if (!float.TryParse(parts[1][0..], NumberStyles.Float, CultureInfo.InvariantCulture, out weight))
            {
                weight = float.Parse(parts[1][1..], NumberStyles.Float, CultureInfo.InvariantCulture);
            }
        }

        NamespacedConfigWeight result = new()
        {
            NamespacedKey = namespacedKey,
            MathOperation = operation,
            Weight = weight
        };
        return result;
    }

    internal static List<NamespacedConfigWeight> ConvertManyFromStringList(List<string> input)
    {
        List<NamespacedConfigWeight> result = new(input.Count);
        foreach (string item in input)
        {
            result.Add(ConvertFromString(item));
        }
        return result;
    }

    internal static List<NamespacedConfigWeight> ConvertManyFromString(string input)
    {
        string[] inputList = input.Split(',');
        List<NamespacedConfigWeight> result = new(inputList.Length);
        foreach (string item in inputList)
        {
            result.Add(ConvertFromString(item));
        }
        return result;
    }
}