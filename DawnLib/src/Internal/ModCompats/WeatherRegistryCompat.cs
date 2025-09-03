using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using WeatherRegistry;

namespace CodeRebirthLib.Internal.ModCompats;

static class WeatherRegistryCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(PluginInfo.PLUGIN_GUID);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Init()
    {
        EventManager.WeatherChanged.AddListener(WeatherRegistry_EventManager_WeatherChanged);
    }

    private static void WeatherRegistry_EventManager_WeatherChanged((SelectableLevel selectableLevel, Weather weather) args)
    {
        EnemyRegistrationHandler.UpdateEnemyWeightsOnLevel(args.selectableLevel);
        ItemRegistrationHandler.UpdateItemWeightsOnLevel(args.selectableLevel);
        MapObjectRegistrationHandler.UpdateInsideMapObjectSpawnWeightsOnLevel(args.selectableLevel);
    }
}