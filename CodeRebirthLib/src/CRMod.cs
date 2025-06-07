using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
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

    public CRRegistry<CRWeatherDefinition> Weathers = new CRRegistry<CRWeatherDefinition>();
    public CRRegistry<CREnemyDefinition> Enemies = new CRRegistry<CREnemyDefinition>();
    public CRRegistry<CRItemDefinition> Items = new CRRegistry<CRItemDefinition>();
    public CRRegistry<CRMapObjectDefinition> MapObjects = new CRRegistry<CRMapObjectDefinition>();
    public CRRegistry<CRUnlockableDefinition> Unlockables = new CRRegistry<CRUnlockableDefinition>();
    
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