using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Dawn.Internal;
using Dawn.Utils;
using PathfindingLib;
using UnityEngine;

namespace Dawn;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(WeatherRegistry.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LethalConfig.PluginInfo.Guid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LethalQuantities.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(PathfindingLibPlugin.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(TerminalFormatter.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
public class DawnPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;

    private void Awake()
    {
        Logger = base.Logger;
        Debuggers.Bind(Config);
        DawnConfig.Bind(Config);

        if (DawnConfig.CreateTagExport)
        {
            TagExporter.Init();
        }

        NetcodePatcher();
        if (LethalConfigCompat.Enabled)
        {
            LethalConfigCompat.Init();
        }

        if (WeatherRegistryCompat.Enabled)
        {
            WeatherRegistryCompat.Init();
        }

        if (LethalQuantitiesCompat.Enabled)
        {
            LethalQuantitiesCompat.Init();
        }

        if (LethalLevelLoaderCompat.Enabled)
        {
            LethalLevelLoaderCompat.Init();
        }
        if (TerminalFormatterCompat.Enabled)
        {
            TerminalFormatterCompat.Init();
        }

        ExtendedTOML.Init();
        PersistentDataHandler.Init();

        MoonRegistrationHandler.Init();
        AdditionalTilesRegistrationHandler.Init();
        ItemRegistrationHandler.Init();
        EnemyRegistrationHandler.Init();
        UnlockableRegistrationHandler.Init();
        MapObjectRegistrationHandler.Init();
        WeatherRegistrationHandler.Init();

        EnemyDataPatch.Init();
        ExtraItemEventsPatch.Init();
        MiscFixesPatch.Init();
        SaveDataPatch.Init();
        TerminalPatches.Init();

        DebugPrintRegistryResult("Enemies", LethalContent.Enemies, enemyInfo => enemyInfo.EnemyType.enemyName);
        DebugPrintRegistryResult("Moons", LethalContent.Moons, moonInfo => moonInfo.Level.PlanetName);
        DebugPrintRegistryResult("Items", LethalContent.Items, itemInfo => itemInfo.Item.itemName);
        DebugPrintRegistryResult("Unlockables", LethalContent.Unlockables, unlockableInfo => unlockableInfo.UnlockableItem.unlockableName);
        DebugPrintRegistryResult("Map Objects", LethalContent.MapObjects, mapObjectInfo => mapObjectInfo.MapObject.name);
        DebugPrintRegistryResult("Weathers", LethalContent.Weathers, weatherInfo => weatherInfo.WeatherEffect.name);
        DebugPrintRegistryResult("Tile Sets", LethalContent.TileSets, tileInfo => tileInfo.TileSet.name);
        DebugPrintRegistryResult("Dungeons", LethalContent.Dungeons, dungeonInfo => dungeonInfo.DungeonFlow.name);
        DebugPrintRegistryResult("Archetypes", LethalContent.Archetypes, archetypeInfo => archetypeInfo.DungeonArchetype.name);


        DawnLib.ApplyAllTagsInFolder(RelativePath("data", "tags"));
    }

    internal static string RelativePath(params string[] folders)
    {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine(folders));
    }

    private void NetcodePatcher()
    {
        var types = new Type[] { typeof(SmartAgentNavigator), typeof(DawnNetworker), typeof(ClientNetworkTransform), typeof(OwnerNetworkAnimator), typeof(ChanceScript), typeof(ApplyRendererVariants), typeof(NetworkAudioSource) };
        foreach (var type in types)
        {
            try
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning($"supressed an error from netcode patcher, probably fine but should still log that something happened: {e}.");
            }
        }
    }

    static void DebugPrintRegistryResult<T>(string name, Registry<T> registry, Func<T, string> nameGetter) where T : INamespaced<T>
    {
        registry.OnFreeze += () =>
        {
            Logger.LogDebug($"Registry '{name}' ({typeof(T).Name}) contains '{registry.Count}' entries.");
            foreach ((NamespacedKey<T> key, T value) in registry)
            {
                Logger.LogDebug($"{key} -> {nameGetter(value)}");
            }
        };
    }
}