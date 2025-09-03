using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using DunGen.Graph;
using LethalLevelLoader;

namespace Dawn.Internal;

static class LLLCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(Plugin.ModGUID);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Init()
    {
        // skip LLL because for some unknown reason it chooses to just remove all scrap with 0 rarity (which is used in CRLib in some cases for dynamic weights)
        // LLL, this is not your job. If you wanted to make sure people didn't register scrap with 0 weight, please check and use your own scriptable objects, don't create behaviour that isn't defined anywhere.
        On.LethalLevelLoader.SafetyPatches.RoundManagerSpawnScrapInLevel_Prefix += orig => true;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool IsExtendedLevel(SelectableLevel level)
    {
        if (LethalLevelLoader.LevelManager.TryGetExtendedLevel(level, out _))
        {
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool IsExtendedDungeon(DungeonFlow dungeonFlow)
    {
        if (LethalLevelLoader.DungeonManager.TryGetExtendedDungeonFlow(dungeonFlow, out _))
        {
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetAllTagsWithModNames(SelectableLevel selectableLevel, out List<(string modName, string tagName)> allTagsWithModNames)
    {
        allTagsWithModNames = new();
        if (!LethalLevelLoader.LevelManager.TryGetExtendedLevel(selectableLevel, out ExtendedLevel extendedLevel))
        {
            return false;
        }

        foreach (ContentTag contentTag in extendedLevel.ContentTags)
        {
            allTagsWithModNames.Add((extendedLevel.ModName, contentTag.contentTagName));
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetAllTagsWithModNames(DungeonFlow dungeonFlow, out List<(string modName, string tagName)> allTagsWithModNames)
    {
        allTagsWithModNames = new();
        if (!LethalLevelLoader.DungeonManager.TryGetExtendedDungeonFlow(dungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
        {
            return false;
        }

        foreach (ContentTag contentTag in extendedDungeonFlow.ContentTags)
        {
            allTagsWithModNames.Add((extendedDungeonFlow.ModName, contentTag.contentTagName));
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetAllTagsWithModNames(Item item, out List<(string modName, string tagName)> allTagsWithModNames)
    {
        allTagsWithModNames = new();
        ExtendedItem? extendedItem = null;
        foreach (ExtendedItem extendedItem1 in LethalLevelLoader.PatchedContent.ExtendedItems)
        {
            if (extendedItem1.Item == item)
            {
                extendedItem = extendedItem1;
                break;
            }
        }

        if (extendedItem == null)
        {
            return false;
        }

        foreach (ContentTag contentTag in extendedItem.ContentTags)
        {
            allTagsWithModNames.Add((extendedItem.ModName, contentTag.contentTagName));
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetAllTagsWithModNames(EnemyType enemyType, out List<(string modName, string tagName)> allTagsWithModNames)
    {
        allTagsWithModNames = new();
        ExtendedEnemyType? extendedEnemyType = null;
        foreach (ExtendedEnemyType extendedEnemy in LethalLevelLoader.PatchedContent.ExtendedEnemyTypes)
        {
            if (extendedEnemy.EnemyType == enemyType)
            {
                extendedEnemyType = extendedEnemy;
                break;
            }
        }

        if (extendedEnemyType == null)
        {
            return false;
        }

        foreach (ContentTag contentTag in extendedEnemyType.ContentTags)
        {
            allTagsWithModNames.Add((extendedEnemyType.ModName, contentTag.contentTagName));
        }
        return true;
    }
}