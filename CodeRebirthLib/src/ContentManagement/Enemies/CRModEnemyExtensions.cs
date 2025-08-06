using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.Extensions;

namespace CodeRebirthLib.ContentManagement.Enemies;
public static class CRModEnemyExtensions
{
    public static bool TryGetFromEnemyName(this IEnumerable<CREnemyDefinition> registry, string enemyName, [NotNullWhen(true)] out CREnemyDefinition? value)
    {
        return registry.TryGetFirstBySomeName(it => it.EnemyType.enemyName,
            enemyName,
            out value,
            $"TryGetFromEnemyName failed with enemyName: {enemyName}"
        );
    }

    public static bool TryGetDefinition(this EnemyType type, [NotNullWhen(true)] out CREnemyDefinition? definition)
    {
        definition = LethalContent.Enemies.CRLib.FirstOrDefault(it => it.EnemyType == type);
        if (!definition) CodeRebirthLibPlugin.ExtendedLogging($"TryGetDefinition for EnemyDefinition failed with {type.enemyName}");
        return definition; // implict cast
    }
}