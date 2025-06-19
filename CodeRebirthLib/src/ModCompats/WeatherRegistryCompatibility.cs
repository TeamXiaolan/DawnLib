using System.Runtime.CompilerServices;
using CodeRebirthLib.ContentManagement.Enemies;

namespace CodeRebirthLib.ModCompats;
static class WeatherRegistryCompatibility
{
    public static bool Enabled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(WeatherRegistry.PluginInfo.PLUGIN_GUID);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Init()
    {
        WeatherRegistry.EventManager.WeatherChanged.AddListener(WeatherRegistry_EventManager_WeatherChanged);
    }

    private static void WeatherRegistry_EventManager_WeatherChanged((SelectableLevel selectableLevel, WeatherRegistry.Weather weather) args)
    {
        CREnemyDefinition.UpdateAllEnemyWeightsForLevel(args.selectableLevel.Enemies, args.selectableLevel);
        CREnemyDefinition.UpdateAllEnemyWeightsForLevel(args.selectableLevel.OutsideEnemies, args.selectableLevel);
        CREnemyDefinition.UpdateAllEnemyWeightsForLevel(args.selectableLevel.DaytimeEnemies, args.selectableLevel);
    }
}