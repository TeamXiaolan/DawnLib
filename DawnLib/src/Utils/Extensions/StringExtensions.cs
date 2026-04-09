using System;
using System.Linq;
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

    private static readonly Regex LeadingNumberRegex = new(@"^[0-9]+");

    public static string RemoveLeadingNumbers(this string input)
    {
        return LeadingNumberRegex.Replace(input, string.Empty);
    }

    public static string ReplaceNumbersWithWords(this string input)
    {
        return string.Concat(input.Select(c => c switch
        {
            '0' => "zero",
            '1' => "one",
            '2' => "two",
            '3' => "three",
            '4' => "four",
            '5' => "five",
            '6' => "six",
            '7' => "seven",
            '8' => "eight",
            '9' => "nine",
            _ => c.ToString()
        }));
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
    /// Modify a string to insert/replace added content at the position of a specific matching string
    /// </summary>
    /// <param name="value">Full string being modified</param>
    /// <param name="indexStyle">This will determine what matching text values are modified.</param>
    /// <param name="insertStyle">This will determine whether we replace the matching string completely or insert our text before/after it</param>
    /// <param name="matching">This is the matching string we are searching for to add our content.</param>
    /// <param name="addedContent">This is the string content we are adding.</param>
    public static string TextModify(this string value, MatchIndex indexStyle, MatchInsert insertStyle, string matching, string addedContent)
    {
        if (string.IsNullOrEmpty(value) || !value.Contains(matching))
        {
            return value;
        }

        if (indexStyle is MatchIndex.All)
        {
            return insertStyle switch
            {
                MatchInsert.ReplaceMatch => value.Replace(matching, addedContent),
                MatchInsert.Before => value.Replace(matching, addedContent + matching),
                MatchInsert.After => value.Replace(matching, matching + addedContent),
                MatchInsert.Before | MatchInsert.After => value.Replace(matching, addedContent + matching + addedContent),
                _ => value,
            };
        }
        else
        {
            // depending on the style will either return the first or last index value of the textToFind
            int index = (indexStyle is MatchIndex.First) ? value.IndexOf(matching) : value.LastIndexOf(matching);

            return insertStyle switch
            {
                MatchInsert.ReplaceMatch => value.Remove(index, matching.Length).Insert(index, addedContent),
                MatchInsert.Before => value.Insert(index, addedContent),
                MatchInsert.After => value.Insert(index + matching.Length, addedContent),
                MatchInsert.Before | MatchInsert.After => value.Remove(index, matching.Length).Insert(index, addedContent + matching + addedContent),
                _ => value,
            };
        }
    }

    private static readonly Regex ConfigCleanerRegex = new(@"[\n\t""`\[\]']");
    internal static string CleanStringForConfig(this string input)
    {
        // The regex pattern matches: newline, tab, double quote, backtick, apostrophe, [ or ].
        return ConfigCleanerRegex.Replace(input, string.Empty);
    }
}