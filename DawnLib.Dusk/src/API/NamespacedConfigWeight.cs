using System;
using System.Globalization;
using Dawn;
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
    public int Weight = 0;

    internal static string ConvertToString(NamespacedConfigWeight input)
    {
        // End Result: {NamespacedKey}={Operation}{Weight}
        if (input == null || input.NamespacedKey == null)
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

    internal static NamespacedConfigWeight ConvertFromString(string input)
    {
        string[] parts = input.Split('=');
        NamespacedKey namespacedKey = NamespacedKey.ForceParse(parts[0]);
        MathOperation operation = MathOperation.Additive;
        int weight = 0;
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
            weight = (int)float.Parse(parts[1][1..], NumberStyles.Float, CultureInfo.InvariantCulture);
        }
        NamespacedConfigWeight result = new()
        {
            NamespacedKey = namespacedKey,
            MathOperation = operation,
            Weight = weight
        };
        return result;
    }
}