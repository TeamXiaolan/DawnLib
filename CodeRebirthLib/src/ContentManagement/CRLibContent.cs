using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ContentManagement.Dungeons;
using CodeRebirthLib.ContentManagement.Enemies;

namespace CodeRebirthLib.ContentManagement;

public class CRLibContent
{
    public static IEnumerable<CREnemyDefinition> AllEnemies()
    {
        return CRMod.AllMods.SelectMany(mod => mod.EnemyRegistry());
    }

    public static IEnumerable<CRAdditionalTilesDefinition> AllAdditionalTiles()
    {
        return CRMod.AllMods.SelectMany(mod => mod.AdditionalTilesRegistry());
    }
}