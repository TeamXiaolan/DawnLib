using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using DunGen.Graph;
using HarmonyLib;
using LethalLevelLoader;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Dawn.Internal;

static class LethalLevelLoaderCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(Plugin.ModGUID);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void ScrewWithLLLDynamicDungeonRarity()
    {
        // DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(LethalLevelLoader.DungeonMatchingProperties), "GetDynamicRarity"), EnsureCorrectDawnDungeonDynamicRarity));
        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(LethalLevelLoader.LevelMatchingProperties), "GetDynamicRarity"), EnsureCorrectDawnDungeonDynamicRarity));
    }

    private static int EnsureCorrectDawnDungeonDynamicRarity(RuntimeILReferenceBag.FastDelegateInvokers.Func<LevelMatchingProperties, ExtendedLevel, int> orig, LevelMatchingProperties self, ExtendedLevel extendedLevel)
    {
        ExtendedDungeonFlow? extendedDungeonFlow = LethalLevelLoader.PatchedContent.ExtendedDungeonFlows.Where(x => x.LevelMatchingProperties.Equals(self)).FirstOrDefault();
        DungeonFlow? dungeonFlow = extendedDungeonFlow?.DungeonFlow;
        if (dungeonFlow != null && dungeonFlow.HasDawnInfo() && !dungeonFlow.GetDawnInfo().ShouldSkipRespectOverride())
        {
            return dungeonFlow.GetDawnInfo().Weights.GetFor(extendedLevel.SelectableLevel.GetDawnInfo()) ?? 0;
        }
        return orig(self, extendedLevel);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void TryRemoveLLLDungeonTranspiler()
    {
        MethodBase originalMethod = typeof(RoundManager).GetMethod(nameof(RoundManager.GenerateNewFloor));
        DawnPlugin._harmony.Unpatch(originalMethod, HarmonyLib.HarmonyPatchType.Transpiler, LethalLevelLoader.Plugin.ModGUID);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetExtendedLevelModName(SelectableLevel level, out string modName)
    {
        modName = "lethal_level_loader";
        if (LethalLevelLoader.LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
        {
            modName = extendedLevel.ModName;
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool ExtendedLevelIsModded(SelectableLevel level, [NotNullWhen(true)] out object? extendedLevelObject)
    {
        extendedLevelObject = null;
        if (LethalLevelLoader.LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
        {
            extendedLevelObject = extendedLevel;
            return extendedLevel.ContentType == ContentType.Custom;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetExtendedDungeonModName(DungeonFlow dungeonFlow, out string modName)
    {
        modName = "lethal_level_loader";
        if (LethalLevelLoader.DungeonManager.TryGetExtendedDungeonFlow(dungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
        {
            modName = extendedDungeonFlow.ModName;
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetExtendedEnemyTypeModName(EnemyType enemyType, out string modName)
    {
        modName = "lethal_level_loader";
        foreach (ExtendedEnemyType extendedEnemy in LethalLevelLoader.PatchedContent.ExtendedEnemyTypes)
        {
            if (extendedEnemy.EnemyType == enemyType)
            {
                modName = extendedEnemy.ModName;
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetExtendedItemModName(Item item, out string modName)
    {
        modName = "lethal_level_loader";
        foreach (ExtendedItem extendedItem in LethalLevelLoader.PatchedContent.ExtendedItems)
        {
            if (extendedItem.Item == item)
            {
                modName = extendedItem.ModName;
                return true;
            }
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