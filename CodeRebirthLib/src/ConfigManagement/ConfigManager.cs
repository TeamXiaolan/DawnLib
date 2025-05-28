using System.Text.RegularExpressions;
using BepInEx.Configuration;

namespace CodeRebirthLib.ConfigManagement;
public class ConfigManager(ConfigFile file)
{
    private static readonly Regex ConfigCleanerRegex = new Regex(@"[\n\t""`\[\]']");
    static string CleanStringForConfig(string input)
    {
        // The regex pattern matches: newline, tab, double quote, backtick, apostrophe, [ or ].
        return ConfigCleanerRegex.Replace(input, string.Empty);
    }
}