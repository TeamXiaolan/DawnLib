using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.ContentManagement.Unlockables;

namespace CodeRebirthLib.ContentManagement.Enemies;
public static class CRModUnlockableExtensions
{
    public static bool TryGetFromUnlockableName(this CRRegistry<CRUnlockableDefinition> registry, string unlockableName, [NotNullWhen(true)] out CRUnlockableDefinition? value)
    {
        return registry.TryGetFirstBySomeName(it => it.UnlockableItemDef.unlockable.unlockableName, unlockableName, out value);
    }

    public static bool TryGetDefinition(this UnlockableItem type, [NotNullWhen(true)] out CRUnlockableDefinition? definition)
    {
        definition = CRMod.AllUnlockables().FirstOrDefault(it => it.UnlockableItemDef.unlockable == type);
        return definition; // implict cast
    }
}