using System;
using System.Text.RegularExpressions;

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

    /// <summary>
    /// Replace matching string with a replacement string
    /// </summary>
    /// <param name="value">Full string being modified</param>
    /// <param name="indexStyle">This will determine what matching text values are modified.</param>
    /// <param name="matching">This is the matching string we are finding and replacing</param>
    /// <param name="replacement">This is the string content we are replacing the matching string with</param>
    public static string TextReplacer(this string value, TextIndex indexStyle, string matching, string replacement)
    {
        if (string.IsNullOrEmpty(value) || !value.Contains(matching))
        {
            //DawnPlugin.Logger.LogWarning($"TextReplacer: Unable to find expected text - {textToReplace} in string - {value}. Text remains unchanged");
            return value;
        }

        if (indexStyle is TextIndex.EveryIndex)
        {
            // replace every instance of our matching string
            return value.Replace(matching, replacement);
        }
        else
        {
            // depending on the style will either return the first or last index value of the textToReplace
            int index = (indexStyle is TextIndex.FirstIndex) ? value.IndexOf(matching) : value.LastIndexOf(matching);
            return value.Remove(index, matching.Length).Insert(index, replacement);
        }
    }

    /// <summary>
    /// Add text after a matching string value
    /// </summary>
    /// <param name="value">Full string being modified</param>
    /// <param name="indexStyle">This will determine what matching text values are modified.</param>
    /// <param name="matching">This is the matching string we are finding and adding content after</param>
    /// <param name="addedContent">This is the string content we are adding after the matching string</param>
    public static string TextAdder(this string value, TextIndex indexStyle, string matching, string addedContent)
    {
        if (string.IsNullOrEmpty(value) || !value.Contains(matching))
        {
            //DawnPlugin.Logger.LogWarning($"TextAdder: Unable to find expected text - {textToFind} in string - {value}. Text remains unchanged");
            return value;
        }

        if (indexStyle is TextIndex.EveryIndex)
        {
            // add content to every instance of our matching string
            return value.Replace(matching, matching + addedContent);
        }
        else
        {
            // depending on the style will either return the first or last index value of the textToFind
            int index = (indexStyle is TextIndex.FirstIndex) ? value.IndexOf(matching) : value.LastIndexOf(matching);
            return value.Insert(index + matching.Length, addedContent);
        }
    }

    private static readonly Regex ConfigCleanerRegex = new(@"[\n\t""`\[\]']");
    internal static string CleanStringForConfig(this string input)
    {
        // The regex pattern matches: newline, tab, double quote, backtick, apostrophe, [ or ].
        return ConfigCleanerRegex.Replace(input, string.Empty);
    }
}