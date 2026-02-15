using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Dawn.Internal;
using Dawn.Utils;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using PathfindingLib;

namespace Dawn;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(WeatherRegistry.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LethalConfig.PluginInfo.Guid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LethalQuantities.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(PathfindingLibPlugin.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(TerminalFormatter.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.github.WhiteSpike.MoonDaySpeedMultiplierPatcher", BepInDependency.DependencyFlags.SoftDependency)]
public class DawnPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static PersistentDataContainer PersistentData { get; private set; } = null!;
    internal static readonly List<Hook> Hooks = new();
    internal static readonly Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        Logger = base.Logger;
        Debuggers.Bind(Config);
        DawnConfig.Bind(Config);

        if (DawnConfig.CreateTagExport.Value)
        {
            TagExporter.Init();
        }

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

        if (TerminalFormatterCompat.Enabled)
        {
            TerminalFormatterCompat.Init();
        }

        ExtendedTOML.Init();
        PersistentDataHandler.Init();
        NetworkVariableInitalizer.Init();

        SurfaceRegistrationHandler.Init();
        StoryLogRegistrationHandler.Init();
        MoonRegistrationHandler.Init();
        DungeonRegistrationHandler.Init();
        ItemRegistrationHandler.Init();
        EnemyRegistrationHandler.Init();
        UnlockableRegistrationHandler.Init();
        MapObjectRegistrationHandler.Init();
        WeatherRegistrationHandler.Init();
        HandleCorruptedDataPatch.Init();
        TerminalCommandRegistration.Init();

        EnemyDataPatch.Init();
        ExtraItemEventsPatch.Init();
        MiscFixesPatch.Init();
        SaveDataPatch.Init();
        TerminalPatches.Init();
        DebugPatches.Init();

        // Testing
        DawnTesting.TestCommands();

        _harmony.PatchAll(Assembly.GetExecutingAssembly());

        DawnNetworkSceneManager.Init();

        DebugPrintRegistryResult("Enemies", LethalContent.Enemies, enemyInfo => enemyInfo.EnemyType.enemyName);
        DebugPrintRegistryResult("Moons", LethalContent.Moons, moonInfo => moonInfo.Level.PlanetName);
        DebugPrintRegistryResult("Items", LethalContent.Items, itemInfo => itemInfo.Item.itemName);
        DebugPrintRegistryResult("Unlockables", LethalContent.Unlockables, unlockableInfo => unlockableInfo.UnlockableItem.unlockableName);
        DebugPrintRegistryResult("Map Objects", LethalContent.MapObjects, mapObjectInfo => mapObjectInfo.MapObject.name);
        DebugPrintRegistryResult("Weathers", LethalContent.Weathers, weatherInfo => weatherInfo.WeatherEffect.name);
        DebugPrintRegistryResult("Tile Sets", LethalContent.TileSets, tileInfo => tileInfo.TileSet.name);
        DebugPrintRegistryResult("Dungeons", LethalContent.Dungeons, dungeonInfo => dungeonInfo.DungeonFlow.name);
        DebugPrintRegistryResult("Archetypes", LethalContent.Archetypes, archetypeInfo => archetypeInfo.DungeonArchetype.name);
        DebugPrintRegistryResult("Story Logs", LethalContent.StoryLogs, storyLogInfo => storyLogInfo.StoryLogTerminalNode.creatureName);
        DebugPrintRegistryResult("Surfaces", LethalContent.Surfaces, surfaceInfo => surfaceInfo.Surface.surfaceTag);
        DebugPrintRegistryResult("Terminal Commands", LethalContent.TerminalCommands, commandInfo => commandInfo.CommandBasicInformation.CommandName);

        PersistentData = this.GetPersistentDataContainer();
        DawnLib.ApplyAllTagsInFolder(RelativePath("data", "tags"));
    }

    internal static string RelativePath(params string[] folders)
    {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine(folders));
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