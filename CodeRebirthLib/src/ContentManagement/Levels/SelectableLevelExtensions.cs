using System.Linq;

namespace CodeRebirthLib.ContentManagement.Levels;
public static class SelectableLevelExtensions
{
    public static bool IsVanilla(this SelectableLevel level)
    {
        return LethalContent.Levels.Vanilla.Contains(level);
    }
}