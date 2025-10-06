using System.Linq;

namespace Dawn.Utils;

public static class StringExtensions
{
    public static string ToCapitalized(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input.First().ToString().ToUpper() + input[1..];
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
}