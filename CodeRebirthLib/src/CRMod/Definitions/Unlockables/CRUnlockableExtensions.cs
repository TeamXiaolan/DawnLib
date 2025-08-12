using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.CRMod;

namespace CodeRebirthLib;
public static class CRUnlockableExtensions
{
    public static bool TryGetDefinition(this UnlockableItem type, [NotNullWhen(true)] out CRUnlockableDefinition? definition)
    {
        definition = LethalContent.Unlockables.Values.FirstOrDefault(it => it.UnlockableItem == type);
        if (!definition) Debuggers.ReplaceThis?.Log($"TryGetDefinition for UnlockableDefinition failed with {type.unlockableName}");
        return definition; // implict cast
    }
}