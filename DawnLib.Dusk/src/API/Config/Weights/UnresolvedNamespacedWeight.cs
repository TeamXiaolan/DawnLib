using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Dusk.Weights;

[Serializable]
public readonly struct UnresolvedNamespacedWeight : IOperationWithValue
{
    public string KeyInput { get; }

    public MathOperation Operation { get; }

    public float Value { get; }

    public UnresolvedNamespacedWeight(string keyInput, MathOperation operation, float value)
    {
        KeyInput = keyInput;
        Operation = operation;
        Value = value;
    }

    public override string ToString()
    {
        string operation = Operation switch
        {
            MathOperation.Additive => "+",
            MathOperation.Subtractive => "-",
            MathOperation.Multiplicative => "*",
            MathOperation.Divisive => "/",
            _ => "+"
        };

        return $"{KeyInput}={operation}{Value}";
    }

    public static List<UnresolvedNamespacedWeight> ConvertManyFromString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<UnresolvedNamespacedWeight>();
        }

        string[] inputList = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
        List<UnresolvedNamespacedWeight> result = new(inputList.Length);

        foreach (string item in inputList)
        {
            result.Add(ConvertFromString(item.Trim()));
        }

        return result;
    }

    public static UnresolvedNamespacedWeight ConvertFromString(string input)
    {
        string[] parts = input.Split('=', StringSplitOptions.RemoveEmptyEntries);

        string keyInput = parts[0].Trim();

        MathOperation operation = MathOperation.Additive;
        float weight = 0;

        if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
        {
            string rawWeight = parts[1].Trim();

            operation = rawWeight[0] switch
            {
                '+' => MathOperation.Additive,
                '-' => MathOperation.Subtractive,
                '*' => MathOperation.Multiplicative,
                '/' => MathOperation.Divisive,
                _ => MathOperation.Additive,
            };

            if (!float.TryParse(rawWeight, NumberStyles.Float, CultureInfo.InvariantCulture, out weight))
            {
                if (rawWeight.Length <= 1 || !float.TryParse(rawWeight[1..], NumberStyles.Float, CultureInfo.InvariantCulture, out weight))
                {
                    DuskPlugin.Logger.LogWarning($"Invalid weight value in config input: {input}");
                    weight = 0;
                }
            }
        }

        return new UnresolvedNamespacedWeight(keyInput, operation, Mathf.Abs(weight));
    }
}