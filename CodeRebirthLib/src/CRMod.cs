using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ContentManagement;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;
using CodeRebirthLib.ContentManagement.MapObjects;
using CodeRebirthLib.ContentManagement.Unlockables;
using CodeRebirthLib.ContentManagement.Weathers;
using CodeRebirthLib.Exceptions;
using CodeRebirthLib.Extensions;
using CodeRebirthLib.ModCompats;
using UnityEngine;

namespace CodeRebirthLib;
public class CRMod
{
    private static readonly List<CRMod> _allMods = new();

    private readonly string _basePath;

    private readonly Dictionary<string, CRRegistry> _registries = new();

    // todo: i dont like how many arguments are here lmao
    internal CRMod(Assembly assembly, BaseUnityPlugin plugin, AssetBundle mainBundle, string basePath, ConfigManager configManager) : this(MetadataHelper.GetMetadata(plugin.GetType()), mainBundle, basePath, configManager)
    {
        Assembly = assembly;
    }

    internal CRMod(BepInPlugin plugin, AssetBundle mainBundle, string basePath, ConfigManager configManager)
    {
        ConfigManager = configManager;
        _basePath = basePath;
        Plugin = plugin;

        ContentContainer[] containers = mainBundle.LoadAllAssets<ContentContainer>();
        if (containers.Length == 0)
        {
            throw new NoContentDefinitionInBundle(mainBundle);
        }
        if (containers.Length >= 2)
        {
            throw new MultipleContentDefinitionsInBundle(mainBundle);
        }

        Content = containers[0];

        AddDefaultRegistries();
        if (WeatherRegistryCompatibility.Enabled)
        {
            AddWeatherRegistry();
        }

        _allMods.Add(this);
    }
    public static IReadOnlyList<CRMod> AllMods => _allMods.AsReadOnly();

    public ConfigManager ConfigManager { get; }
    public ContentContainer Content { get; }

    public Assembly? Assembly { get; }
    public ManualLogSource? Logger { get; set; }

    public BepInPlugin Plugin { get; }

    public string GetRelativePath(params string[] path)
    {
        return Path.Combine(_basePath, Path.Combine(path));
    }

    public void CreateRegistry<T>(string name, CRRegistry<T> registry) where T : CRContentDefinition
    {
        _registries[name] = registry;
        CodeRebirthLibPlugin.ExtendedLogging($"Created Registry: {name}");
    }

    public CRRegistry<T> GetRegistryByName<T>(string name) where T : CRContentDefinition
    {
        return (CRRegistry<T>)_registries[name];
    }

    public CRRegistry<CREnemyDefinition> EnemyRegistry()
    {
        return GetRegistryByName<CREnemyDefinition>(CREnemyDefinition.REGISTRY_ID);
    }
    public static IEnumerable<CREnemyDefinition> AllEnemies()
    {
        return AllMods.SelectMany(mod => mod.EnemyRegistry());
    }

    public CRRegistry<CRItemDefinition> ItemRegistry()
    {
        return GetRegistryByName<CRItemDefinition>(CRItemDefinition.REGISTRY_ID);
    }
    public static IEnumerable<CRItemDefinition> AllItems()
    {
        return AllMods.SelectMany(mod => mod.ItemRegistry());
    }

    public CRRegistry<CRMapObjectDefinition> MapObjectRegistry()
    {
        return GetRegistryByName<CRMapObjectDefinition>(CRMapObjectDefinition.REGISTRY_ID);
    }
    public static IEnumerable<CRMapObjectDefinition> AllMapObjects()
    {
        return AllMods.SelectMany(mod => mod.MapObjectRegistry());
    }

    public CRRegistry<CRUnlockableDefinition> UnlockableRegistry()
    {
        return GetRegistryByName<CRUnlockableDefinition>(CRUnlockableDefinition.REGISTRY_ID);
    }
    public static IEnumerable<CRUnlockableDefinition> AllUnlockables()
    {
        return AllMods.SelectMany(mod => mod.UnlockableRegistry());
    }

    public bool TryGetBundleDataFromName(string bundleName, [NotNullWhen(true)] out AssetBundleData? data)
    {
        data = Content.assetBundles.FirstOrDefault(it => it.assetBundleName == bundleName);
        return data != null;
    }

    private void AddDefaultRegistries()
    {
        CREnemyDefinition.RegisterTo(this);
        CRMapObjectDefinition.RegisterTo(this);
        CRItemDefinition.RegisterTo(this);
        CRUnlockableDefinition.RegisterTo(this);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private void AddWeatherRegistry()
    {
        CRWeatherDefinition.RegisterTo(this);
    }

    public void RegisterContentHandlers()
    {
        if (Assembly == null)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"Tried to Register Content Handlers for {Plugin.Name} but it is a no-code CRMod!");
            return;
        }

        IEnumerable<Type> contentHandlers = Assembly.GetLoadableTypes().Where(x =>
            !x.IsNested && x.BaseType != null
            && x.BaseType.IsGenericType
            && x.BaseType.GetGenericTypeDefinition() == typeof(ContentHandler<>)
        );

        foreach (Type type in contentHandlers)
        {
            type.GetConstructor([typeof(CRMod)]).Invoke([this]);
        }
    }
}