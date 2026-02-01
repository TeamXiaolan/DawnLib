namespace Dawn.SourceGen.Extensions;

public static class StringExtensions
{
    public static string ToCapitalized(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Relying on a bold but practical assumption that 1 char == 1 grapheme
        return input.Substring(0, 1).ToUpperInvariant() + input.Substring(1);
    }
}