using System.Collections.Generic;
using System.Linq;

namespace CodeRebirthLib.ContentManagement.Levels;

public static class SelectableLevelExtensions
{
    public static bool IsVanilla(this SelectableLevel level)
    {
        return LethalContent.Levels.Vanilla.Contains(level);
    }

    public static IEnumerable<SpawnableEnemyWithRarity> GetUsedEnemyTypes(this SelectableLevel level)
    {
        foreach (SpawnableEnemyWithRarity enemy in level.DaytimeEnemies)
        {
            yield return enemy;
        }
        foreach (SpawnableEnemyWithRarity enemy in level.OutsideEnemies)
        {
            yield return enemy;
        }
        foreach (SpawnableEnemyWithRarity enemy in level.Enemies)
        {
            yield return enemy;
        }
    }
}