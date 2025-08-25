using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using DunGen.Graph;
using LethalLevelLoader;

namespace CodeRebirthLib.Internal.ModCompats;

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
    public static bool TryGetExtendedLevel(SelectableLevel level, out ExtendedLevel? extendedLevel)
    {
        extendedLevel = null;
        if (LethalLevelLoader.LevelManager.TryGetExtendedLevel(level, out extendedLevel))
        {
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetExtendedDungeon(DungeonFlow dungeonFlow, out ExtendedDungeonFlow? extendedDungeon)
    {
        extendedDungeon = null;
        if (LethalLevelLoader.DungeonManager.TryGetExtendedDungeonFlow(dungeonFlow, out extendedDungeon))
        {
            return true;
        }
        return false;
    }
}