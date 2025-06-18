using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.ContentManagement.Items;

namespace CodeRebirthLib.ContentManagement.Enemies;
public static class CRModItemExtensions
{
    public static bool TryGetFromItemName(this CRRegistry<CRItemDefinition> registry, string itemName, [NotNullWhen(true)] out CRItemDefinition? value)
    {
        return registry.TryGetFirstBySomeName(it => it.Item.itemName, itemName, out value);
    }

    public static bool TryGetDefinition(this Item type, [NotNullWhen(true)] out CRItemDefinition? definition)
    {
        definition = CRMod.AllItems().FirstOrDefault(it => it.Item == type);
        return definition; // implict cast
    }
}