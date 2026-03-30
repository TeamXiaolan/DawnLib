using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Dawn;
using Dawn.Internal;
using Dawn.Utils;
using Dusk.Weights;
using UnityEngine;

namespace Dusk;
[Serializable]
public class IntComparison
{
    [field: SerializeField]
    public int Value = 0;

    [field: SerializeField]
    public ComparisonOperation ComparisonOperation = ComparisonOperation.Equal;
}

[Serializable]
public class IntComparisonConfigWeight : IOperationWithValue
{
    [field: SerializeField]
    public IntComparison IntComparison;

    [field: SerializeField]
    public MathOperation MathOperation = MathOperation.Additive;

    [field: SerializeField]
    [field: Range(-9999, 9999)]
    public float Weight = 0;

    public static string ConvertToString(IntComparisonConfigWeight input)
    {
        // End Result: {Comparison}{Value}={Operation}{Weight}

        string Operation;
        Operation = input.MathOperation switch
        {
            MathOperation.Additive => "+",
            MathOperation.Subtractive => "-",
            MathOperation.Multiplicative => "*",
            MathOperation.Divisive => "/",
            _ => "+",
        };

        string Comparison = input.IntComparison.ComparisonOperation switch
        {
            ComparisonOperation.Equal => "==",
            ComparisonOperation.NotEqual => "!=",
            ComparisonOperation.Greater => ">",
            ComparisonOperation.Less => "<",
            ComparisonOperation.GreaterOrEqual => ">=",
            ComparisonOperation.LessOrEqual => "<=",
            _ => "==",
        };

        string result = $"{Comparison}{input.IntComparison.Value}={Operation}{input.Weight}";
        return result;
    }

    public static string ConvertManyToString(List<IntComparisonConfigWeight> input)
    {
        string result = string.Empty;
        foreach (IntComparisonConfigWeight item in input)
        {
            result += $"{ConvertToString(item)},";
        }

        result = result.RemoveEnd(",");
        return result;
    }

    public static IntComparisonConfigWeight ConvertFromString(string input)
    {
        Debuggers.Weights?.Log($"Converting IntComparisonConfigWeight from string: {input}");

        if (string.IsNullOrWhiteSpace(input))
        {
            DuskPlugin.Logger.LogWarning("Input string was null or empty.");
            return new IntComparisonConfigWeight();
        }

        Match match = Regex.Match(
            input.Trim(),
            @"^(==|!=|<=|>=|<|>)(-?\d+)=([\+\-\*/])(-?\d*\.?\d+)$"
        );

        if (!match.Success)
        {
            DuskPlugin.Logger.LogWarning($"Invalid IntComparisonConfigWeight format: {input}");
            return new IntComparisonConfigWeight();
        }

        string comparisonToken = match.Groups[1].Value;
        string valueToken = match.Groups[2].Value;
        string mathToken = match.Groups[3].Value;
        string weightToken = match.Groups[4].Value;

        ComparisonOperation comparisonOperation = comparisonToken switch
        {
            "==" => ComparisonOperation.Equal,
            "!=" => ComparisonOperation.NotEqual,
            "<"  => ComparisonOperation.Less,
            ">"  => ComparisonOperation.Greater,
            "<=" => ComparisonOperation.Less | ComparisonOperation.Equal,
            ">=" => ComparisonOperation.Greater | ComparisonOperation.Equal,
            _ => ComparisonOperation.Equal
        };

        if (!int.TryParse(valueToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            DuskPlugin.Logger.LogWarning($"Invalid comparison value in input: {input}");
            return new IntComparisonConfigWeight();
        }

        MathOperation mathOperation = mathToken[0] switch
        {
            '+' => MathOperation.Additive,
            '-' => MathOperation.Subtractive,
            '*' => MathOperation.Multiplicative,
            '/' => MathOperation.Divisive,
            _ => MathOperation.Additive,
        };

        if (!float.TryParse(weightToken, NumberStyles.Float, CultureInfo.InvariantCulture, out float weight))
        {
            DuskPlugin.Logger.LogWarning($"Invalid weight value in input: {input}");
            weight = 0f;
        }

        IntComparisonConfigWeight result = new()
        {
            IntComparison = new IntComparison
            {
                Value = value,
                ComparisonOperation = comparisonOperation
            },
            MathOperation = mathOperation,
            Weight = Mathf.Abs(weight)
        };

        Debuggers.Weights?.Log($"Converted IntComparisonConfigWeight: {ConvertToString(result)}");
        return result;
    }

    public static List<IntComparisonConfigWeight> ConvertManyFromStringList(List<string> input)
    {
        List<IntComparisonConfigWeight> result = new(input.Count);
        foreach (string item in input)
        {
            result.Add(ConvertFromString(item.Trim().ToLowerInvariant().Replace(" ", "_")));
        }
        return result;
    }

    private static Regex _splitRegex = new(@"\s+");

    public MathOperation Operation => MathOperation;

    public float Value => Weight;

    public static List<IntComparisonConfigWeight> ConvertManyFromString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<IntComparisonConfigWeight>();
        }

        string[] inputList = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
        List<IntComparisonConfigWeight> result = new List<IntComparisonConfigWeight>(inputList.Length);
        foreach (string item in inputList)
        {
            string normalized = _splitRegex.Replace(item.Trim().ToLowerInvariant(), "_");
            result.Add(ConvertFromString(normalized));
        }
        return result;
    }
}