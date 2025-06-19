using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CodeRebirthLib.ContentManagement.Unlockables;
public static class CRModUnlockableExtensions
{
    public static bool TryGetFromUnlockableName(this CRRegistry<CRUnlockableDefinition> registry, string unlockableName, [NotNullWhen(true)] out CRUnlockableDefinition? value)
    {
        return registry.TryGetFirstBySomeName(
            it => it.UnlockableItemDef.unlockable.unlockableName, 
            unlockableName, 
            out value,
            $"TryGetFromUnlockableName failed with unlockableName: {unlockableName}"
        );
    }

    public static bool TryGetDefinition(this UnlockableItem type, [NotNullWhen(true)] out CRUnlockableDefinition? definition)
    {
        definition = CRMod.AllUnlockables().FirstOrDefault(it => it.UnlockableItemDef.unlockable == type);
        if(!definition) CodeRebirthLibPlugin.ExtendedLogging($"TryGetDefinition for UnlockableDefinition failed with {type.unlockableName}");
        return definition; // implict cast
    }
}