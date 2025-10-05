using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using WeatherRegistry;

namespace Dawn.Internal;

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
        DungeonRegistrationHandler.UpdateDungeonWeightOnLevel(args.selectableLevel);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetWeatherFromWeatherRegistry(string weatherName, out string modName)
    {
        modName = "weather_registry";
        foreach (Weather weather in WeatherManager.RegisteredWeathers)
        {
            if (weather.Effect.VanillaWeatherEffect.name == weatherName)
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool IsWeatherManagerReady()
    {
        return WeatherManager.IsSetupFinished;
    }
}