using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Dawn;
using Dawn.Internal;
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

    public static string ConvertToString(NamespacedConfigWeight input)
    {
        // End Result: {NamespacedKey}={Operation}{Weight}
        if (input == null || input.NamespacedKey == null || string.IsNullOrWhiteSpace(input.NamespacedKey.Namespace) || string.IsNullOrWhiteSpace(input.NamespacedKey.Key))
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

    public static string ConvertManyToString(List<NamespacedConfigWeight> input)
    {
        string result = string.Empty;
        foreach (NamespacedConfigWeight item in input)
        {
            result += $"{ConvertToString(item)},";
        }

        result = result.RemoveEnd(",");
        return result;
    }

    public static NamespacedConfigWeight ConvertFromString(string input)
    {
        string[] parts = input.Split('=', StringSplitOptions.RemoveEmptyEntries);
        Debuggers.Weights?.Log($"Converting NamespacedConfigWeight from string: {input}");
        NamespacedKey namespacedKey = NamespacedKey.ForceParse(parts[0], true);
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
                if (!float.TryParse(parts[1][1..], NumberStyles.Float, CultureInfo.InvariantCulture, out weight))
                {
                    DuskPlugin.Logger.LogWarning($"Invalid Conversion from string config input to a weight: {input}");
                    weight = 0;
                }
            }
        }

        NamespacedConfigWeight result = new()
        {
            NamespacedKey = namespacedKey,
            MathOperation = operation,
            Weight = Mathf.Abs(weight)
        };
        Debuggers.Weights?.Log($"Converted NamespacedConfigWeight: {ConvertToString(result)}");
        return result;
    }

    public static List<NamespacedConfigWeight> ConvertManyFromStringList(List<string> input)
    {
        List<NamespacedConfigWeight> result = new(input.Count);
        foreach (string item in input)
        {
            result.Add(ConvertFromString(item.Trim().ToLowerInvariant().Replace(" ", "_")));
        }
        return result;
    }

    private static Regex _splitRegex = new(@"\s+");
    public static List<NamespacedConfigWeight> ConvertManyFromString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<NamespacedConfigWeight>();

        string[] inputList = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
        List<NamespacedConfigWeight> result = new List<NamespacedConfigWeight>(inputList.Length);
        foreach (string item in inputList)
        {
            string normalized = _splitRegex.Replace(item.Trim().ToLowerInvariant(), "_");
            result.Add(ConvertFromString(normalized));
        }
        return result;
    }
}