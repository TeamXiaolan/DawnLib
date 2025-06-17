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
        // todo: in theory this should only recalculate for the specific moon, but oh well
        CREnemyDefinition.UpdateAllWeights();
    }
}