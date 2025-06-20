using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CodeRebirthLib.ContentManagement.Items;
public static class CRModItemExtensions
{
    public static bool TryGetFromItemName(this CRRegistry<CRItemDefinition> registry, string itemName, [NotNullWhen(true)] out CRItemDefinition? value)
    {
        return registry.TryGetFirstBySomeName(
            it => it.Item.itemName,
            itemName,
            out value,
            $"TryGetFromItemName failed with itemName: {itemName}"
        );
    }

    public static bool TryGetDefinition(this Item type, [NotNullWhen(true)] out CRItemDefinition? definition)
    {
        definition = CRMod.AllItems().FirstOrDefault(it => it.Item == type);
        if (!definition) CodeRebirthLibPlugin.ExtendedLogging($"TryGetDefinition for ItemDefinition failed with {type.itemName}");
        return definition; // implict cast
    }
}