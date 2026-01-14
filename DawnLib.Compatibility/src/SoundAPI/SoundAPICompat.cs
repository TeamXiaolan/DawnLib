using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using loaforcsSoundAPI;

namespace Dawn.Compatibility;
static class SoundAPICompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(SoundAPI.PLUGIN_GUID);

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    internal static void Init()
    {
        SoundAPI.RegisterCondition("DawnLib:moon:has_tag", () => new DawnTaggableCondition(() =>
        {
            if (!StartOfRound.Instance) return null;
            return StartOfRound.Instance.currentLevel.GetDawnInfo();
        }));

        SoundAPI.RegisterCondition("DawnLib:dungeon:has_tag", () => new DawnTaggableCondition(() =>
        {
            if (!RoundManager.Instance) return null;
            if (!RoundManager.Instance.dungeonGenerator) return null;
            return RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.GetDawnInfo();
        }));
    }
}