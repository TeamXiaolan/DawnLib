using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
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
using UnityEngine;

namespace CodeRebirthLib;
public class CRMod
{
    public ConfigManager ConfigManager { get; }
    public ContentContainer Content { get; private set; }
    
    public Assembly Assembly { get; }
    public ManualLogSource? Logger { get; set; }

    private Dictionary<string, CRRegistry> _registries = new();

    public void CreateRegistry<T>(string name, CRRegistry<T> registry) where T : CRContentDefinition
    {
        _registries[name] = registry;
        CodeRebirthLibPlugin.ExtendedLogging($"Created Registry: {name}");
    }

    public CRRegistry<T> GetRegistryByName<T>(string name) where T : CRContentDefinition
    {
        return (CRRegistry<T>) _registries[name];
    }

    public CRRegistry<CREnemyDefinition> EnemyRegistry()
    {
        return GetRegistryByName<CREnemyDefinition>(CREnemyDefinition.REGISTRY_ID);
    }
    
    public CRRegistry<CRItemDefinition> ItemRegistry()
    {
        return GetRegistryByName<CRItemDefinition>(CRItemDefinition.REGISTRY_ID);
    }
    
    public CRRegistry<CRMapObjectDefinition> MapObjectRegistry()
    {
        return GetRegistryByName<CRMapObjectDefinition>(CRMapObjectDefinition.REGISTRY_ID);
    }
    
    public CRRegistry<CRUnlockableDefinition> UnlockableRegistry()
    {
        return GetRegistryByName<CRUnlockableDefinition>(CRUnlockableDefinition.REGISTRY_ID);
    }
    
    internal CRMod(Assembly assembly, BaseUnityPlugin plugin, AssetBundle mainBundle)
    {
        ConfigManager = new ConfigManager(plugin.Config);
        Assembly = assembly;

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
        if (Chainloader.PluginInfos.ContainsKey(WeatherRegistry.PluginInfo.PLUGIN_GUID))
        {
            AddWeatherRegistry();
        }
    }

    void AddDefaultRegistries()
    {
        CREnemyDefinition.RegisterTo(this);
        CRMapObjectDefinition.RegisterTo(this);
        CRItemDefinition.RegisterTo(this);
        CRUnlockableDefinition.RegisterTo(this);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    void AddWeatherRegistry()
    {
        CRWeatherDefinition.RegisterTo(this);
    }
    
    public void RegisterContentHandlers()
    {
        IEnumerable<Type> contentHandlers = Assembly.GetLoadableTypes().Where(x =>
            x.BaseType != null
            && x.BaseType.IsGenericType
            && x.BaseType.GetGenericTypeDefinition() == typeof(ContentHandler<>)
        );

        foreach (Type type in contentHandlers)
        {
            type.GetConstructor([typeof(CRMod)]).Invoke([this]);
        }
    }
}