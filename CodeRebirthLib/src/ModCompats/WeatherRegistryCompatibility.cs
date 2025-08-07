using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;
using WeatherRegistry;

namespace CodeRebirthLib.ModCompats;
static class WeatherRegistryCompatibility
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(PluginInfo.PLUGIN_GUID);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Init()
    {
        EventManager.WeatherChanged.AddListener(WeatherRegistry_EventManager_WeatherChanged);
    }

    private static void WeatherRegistry_EventManager_WeatherChanged((SelectableLevel selectableLevel, Weather weather) args)
    {
        CREnemyDefinition.UpdateAllWeights(args.selectableLevel);
        CRItemDefinition.UpdateAllWeights(args.selectableLevel);
    }
}