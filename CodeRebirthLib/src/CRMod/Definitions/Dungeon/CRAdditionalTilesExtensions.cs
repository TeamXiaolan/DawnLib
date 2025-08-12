using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.CRMod;
using DunGen;

namespace CodeRebirthLib;
public static class CRModAdditionalTilesExtensions
{
    public static bool TryGetDefinition(this TileSet type, [NotNullWhen(true)] out CRAdditionalTilesDefinition? definition)
    {
        definition = LethalContent.TileSets.Values.FirstOrDefault(it => it.TileSet == type);
        if (!definition) Debuggers.ReplaceThis?.Log($"TryGetDefinition for AdditionalTilesDefinition failed with {type.name}");
        return definition; // implict cast
    }
}