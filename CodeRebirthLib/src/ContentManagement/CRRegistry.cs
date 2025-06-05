using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CodeRebirthLib.AssetManagement;
using System.Linq;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;
using CodeRebirthLib.ContentManagement.MapObjects;
using CodeRebirthLib.ContentManagement.Unlockables;
using CodeRebirthLib.ContentManagement.Weathers;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement;
public class CRRegistry<TDefinition, TData> : IEnumerable<TDefinition> where TData : EntityData where TDefinition : CRContentDefinition<TData>
{
    [SerializeField]
    private List<TDefinition> _items = new();

    public void Register(TDefinition item)
    {
        _items.Add(item);
        CodeRebirthLibPlugin.ExtendedLogging($"added {item.name} to registry.");
    }

    public bool TryGetFirstBySomeName(Func<TDefinition, string> transformer, string name, [NotNullWhen(true)] out TDefinition? value)
    {
        value = this.FirstOrDefault(it => transformer(it).Contains(name, StringComparison.OrdinalIgnoreCase));
        return value;// implicit cast to bool
    }
    
    public bool TryGetFromAssetName(string name, [NotNullWhen(true)] out TDefinition? value)
    {
        return TryGetFirstBySomeName(it => it.name, name, out value);
    }
    
    public IEnumerator<TDefinition> GetEnumerator()
    {
        return _items.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

// This is kind of icky
public static class CRRegistryExtensions
{
    public static bool TryGetFromEnemyName<T>(this CRRegistry<T, EnemyData> registry, string enemyName, [NotNullWhen(true)] out T? value) where T : CREnemyDefinition
    {
        return registry.TryGetFirstBySomeName(it => it.EnemyType.enemyName, enemyName, out value);
    }
    
    /*
    public static bool TryGetFromItemName<T>(this CRRegistry<T, ItemData> registry, string itemName, [NotNullWhen(true)] out T? value) where T : CRItemDefinition
    {
        return registry.TryGetFirstBySomeName(it => it.Item.itemName, itemName, out value);
    }
    
    public static bool TryGetFromMapObjectName<T>(this CRRegistry<T, MapObjectData> registry, string objectName, [NotNullWhen(true)] out T? value) where T : CRMapObjectDefinition
    {
        return registry.TryGetFirstBySomeName(it => it.ObjectName, objectName, out value);
    }
    
    public static bool TryGetFromUnlockableName<T>(this CRRegistry<T, UnlockableData> registry, string unlockableName, [NotNullWhen(true)] out T? value) where T : CRUnlockableDefinition
    {
        return registry.TryGetFirstBySomeName(it => it.UnlockableItemDef.unlockable.unlockableName, unlockableName, out value);
    }
    
    public static bool TryGetFromWeatherName<T>(this CRRegistry<T, WeatherData> registry, string weatherName, [NotNullWhen(true)] out T? value) where T : CRWeatherDefinition
    {
        return registry.TryGetFirstBySomeName(it => it.Weather.Name, weatherName, out value);
    }
    */
}