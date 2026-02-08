namespace Dawn.SourceGen.Tests.Utils;

public class Settings
{
    internal static readonly VerifySettings Instance;

    static Settings()
    {
        Instance = new VerifySettings();
        Instance.UseDirectory("Snapshots");
        // scrub volatile version number
        Instance.ScrubLinesWithReplace(VersionScrubber);
    }

    static string? VersionScrubber(string line)
    {
        const string prefix = "[System.CodeDom.Compiler.GeneratedCode(\"DawnLib\", \"";
        const string replacement = "<version scrubbed>";
        if (line.StartsWith(prefix))
        {
            var end = line.IndexOf('"', prefix.Length);
            if (end != -1)
            {
                return $"{line[..prefix.Length]}{replacement}{line[end..]}";
            }
        }
        return line;
    }
}
