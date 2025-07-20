using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.Extensions;

namespace CodeRebirthLib.ContentManagement.Enemies;
public static class CRModEnemyExtensions
{
    public static bool TryGetFromEnemyName(this CRRegistry<CREnemyDefinition> registry, string enemyName, [NotNullWhen(true)] out CREnemyDefinition? value)
    {
        return CRRegistryExtensions.TryGetFirstBySomeName(registry, 
            it => it.EnemyType.enemyName,
            enemyName,
            out value,
            $"TryGetFromEnemyName failed with enemyName: {enemyName}"
        );
    }

    public static bool TryGetDefinition(this EnemyType type, [NotNullWhen(true)] out CREnemyDefinition? definition)
    {
        definition = CRMod.AllEnemies().FirstOrDefault(it => it.EnemyType == type);
        if (!definition) CodeRebirthLibPlugin.ExtendedLogging($"TryGetDefinition for EnemyDefinition failed with {type.enemyName}");
        return definition; // implict cast
    }
}