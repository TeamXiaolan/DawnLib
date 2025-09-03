using System.Linq;

namespace CodeRebirthLib.SourceGen.Extensions;
public static class StringExtensions
{
    public static string ToCapitalized(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input.First().ToString().ToUpper() + string.Join("", input.Skip(1));
    }
}