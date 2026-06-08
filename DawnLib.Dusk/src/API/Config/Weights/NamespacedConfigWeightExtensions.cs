using System;
using System.Collections.Generic;
using Dawn.Utils;

namespace Dusk.Weights;

public static class NamespacedConfigWeightExtensions
{
    public static UnresolvedNamespacedWeight ToUnresolvedWeight(this NamespacedConfigWeight configWeight)
    {
        if (configWeight.NamespacedKey == null || string.IsNullOrEmpty(configWeight.NamespacedKey.ToString()))
        {
            throw new ArgumentNullException(nameof(configWeight.NamespacedKey) + " cannot be null or empty for parsing weights.");
        }

        string keyInput = configWeight.NamespacedKey.ToString();
        return new UnresolvedNamespacedWeight(keyInput, configWeight.Operation, configWeight.Value);
    }

    public static List<UnresolvedNamespacedWeight> ToUnresolvedWeights(this List<NamespacedConfigWeight> configWeights)
    {
        List<UnresolvedNamespacedWeight> result = new(configWeights.Count);

        foreach (NamespacedConfigWeight configWeight in configWeights)
        {
            result.Add(configWeight.ToUnresolvedWeight());
        }

        return result;
    }

    public static string ConvertToString(this NamespacedConfigWeight namespacedConfigWeight)
    {
        // End Result: {NamespacedKey}={Operation}{Weight}
        if (namespacedConfigWeight == null || namespacedConfigWeight.NamespacedKey == null || string.IsNullOrWhiteSpace(namespacedConfigWeight.NamespacedKey.Namespace) || string.IsNullOrWhiteSpace(namespacedConfigWeight.NamespacedKey.Key))
        {
            DuskPlugin.Logger.LogWarning($"Invalid Conversion from NamespacedConfigWeight to string: {namespacedConfigWeight}");
            return string.Empty;
        }

        string Operation;
        Operation = namespacedConfigWeight.MathOperation switch
        {
            MathOperation.Additive => "+",
            MathOperation.Subtractive => "-",
            MathOperation.Multiplicative => "*",
            MathOperation.Divisive => "/",
            _ => "+",
        };
        string result = $"{namespacedConfigWeight.NamespacedKey}={Operation}{namespacedConfigWeight.Weight}";
        return result;
    }

    public static string ConvertManyToString(this IEnumerable<NamespacedConfigWeight> namespacedConfigWeightList)
    {
        string result = string.Empty;
        foreach (NamespacedConfigWeight item in namespacedConfigWeightList)
        {
            result += $"{ConvertToString(item)},";
        }

        result = result.RemoveEnd(",");
        return result;
    }
}