using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.ContentManagement.Dungeons;
using CodeRebirthLib.Extensions;
using DunGen;

namespace CodeRebirthLib.ContentManagement.Enemies;
public static class CRModAdditionalTilesExtensions
{
    public static bool TryGetFromAdditionalTilesName(this IEnumerable<CRAdditionalTilesDefinition> registry, string AdditionalTilesName, [NotNullWhen(true)] out CRAdditionalTilesDefinition? value)
    {
        return registry.TryGetFirstBySomeName(it => it.ArchetypeName,
            AdditionalTilesName,
            out value,
            $"TryGetFromAdditionalTilesName failed with AdditionalTilesName: {AdditionalTilesName}"
        );
    }

    public static bool TryGetDefinition(this TileSet type, [NotNullWhen(true)] out CRAdditionalTilesDefinition? definition)
    {
        definition = LethalContent.Dungeons.CRLib.FirstOrDefault(it => it.TilesToAdd == type);
        if (!definition) CodeRebirthLibPlugin.ExtendedLogging($"TryGetDefinition for AdditionalTilesDefinition failed with {type.name}");
        return definition; // implict cast
    }
}