using System;
using System.Linq;

namespace Dawn.Utils;

public static class StringExtensions
{
    public static string ToCapitalized(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Relying on a bold but practical assumption that 1 char == 1 grapheme
        return input[..1].ToUpperInvariant() + input[1..];
    }

    public static string RemoveEnd(this string input, string end)
    {
        if (input.EndsWith(end))
        {
            return input[..^end.Length];
        }

        return input;
    }

    public static string StripSpecialCharacters(this string input)
    {
        string returnString = string.Empty;
        foreach (char charmander in input)
        {
            if (!char.IsLetter(charmander))
            {
                continue;
            }

            returnString += charmander;
        }

        return returnString;
    }

    public static bool CompareStringsInvariant(this string input, string str2, bool ignoreCase = true)
    {
        StringComparison comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
        return input.Equals(str2, comparison);
    }

    public static bool StringStartsWithInvariant(this string input, char ch, bool ignoreCase = true)
    {
        StringComparison comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
        return input.StartsWith($"{ch}", comparison);
    }

    public static bool StringStartsWithInvariant(this string input, string str, bool ignoreCase = true)
    {
        StringComparison comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
        return input.StartsWith(str, comparison);
    }

    public static bool StringContainsInvariant(this string input, string query, bool ignoreCase = true)
    {
        StringComparison comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
        return input.Contains(query, comparison);
    }

    public static int StringMatchScore(this string input, string query, bool ignoreCase = true)
    {
        int value = 0;
        if (ignoreCase)
        {
            input = input.ToLowerInvariant();
            query = query.ToLowerInvariant();
        }

        for (int i = 0; i < input.Length; i++)
        {
            if (query.Length <= i)
                break;

            if (input[i].Equals(query[i]))
            {
                value++;
            }
        }

        return value;
    }

    public static string GetExactMatch(this string input, string query, bool ignoreCase = true)
    {
        string result = string.Empty;
        if (ignoreCase)
        {
            input = input.ToLowerInvariant();
            query = query.ToLowerInvariant();
        }

        input = input.Trim();
        query = query.Trim();

        for (int i = 0; i < input.Length; i++)
        {
            if (query.Length <= i)
                break;

            if (input[i].Equals(query[i]))
            {
                result += input[i];
            }
            else
            {
                break;
            }
        }

        return result;
    }

    public static string BepinFriendlyString(this string input)
    {
        char[] invalidChars = ['\'', '\n', '\t', '\\', '"', '[', ']'];
        string result = "";

        input = input.Trim();

        foreach (char c in input)
        {
            if (!invalidChars.Contains(c))
            {
                result += c;
            }
            else
            {
                continue;
            }
        }

        return result;
    }
}