using System;
using System.Linq;

namespace CodeRebirthLib.Extensions;
public static class StringExtensions
{
    public static string ToCapitalized(this string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("ARGH!");
        return input.First().ToString().ToUpper() + input[1..];
    }
}