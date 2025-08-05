using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.Extensions;

namespace CodeRebirthLib.ContentManagement.Unlockables;
public static class CRModUnlockableExtensions
{
    public static bool TryGetFromUnlockableName(this IEnumerable<CRUnlockableDefinition> registry, string unlockableName, [NotNullWhen(true)] out CRUnlockableDefinition? value)
    {
        return registry.TryGetFirstBySomeName(it => it.UnlockableItem.unlockableName,
            unlockableName,
            out value,
            $"TryGetFromUnlockableName failed with unlockableName: {unlockableName}"
        );
    }

    public static bool TryGetDefinition(this UnlockableItem type, [NotNullWhen(true)] out CRUnlockableDefinition? definition)
    {
        definition = CRMod.AllUnlockables().FirstOrDefault(it => it.UnlockableItem == type);
        if (!definition) CodeRebirthLibPlugin.ExtendedLogging($"TryGetDefinition for UnlockableDefinition failed with {type.unlockableName}");
        return definition; // implict cast
    }
}