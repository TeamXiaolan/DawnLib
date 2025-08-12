using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.CRMod;

namespace CodeRebirthLib;
public static class CRModItemExtensions
{
    public static bool TryGetDefinition(this Item type, [NotNullWhen(true)] out CRItemDefinition? definition)
    {
        definition = LethalContent.Items.Values.FirstOrDefault(it => it.Item == type);
        if (!definition) Debuggers.ReplaceThis?.Log($"TryGetDefinition for ItemDefinition failed with {type.itemName}");
        return definition; // implict cast
    }
}