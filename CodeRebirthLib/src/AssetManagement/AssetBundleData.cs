using System;
using System.Collections.Generic;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;
using CodeRebirthLib.ContentManagement.MapObjects;
using CodeRebirthLib.ContentManagement.Unlockables;
using CodeRebirthLib.ContentManagement.Weathers;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.AssetManagement;
[Serializable]
public class AssetBundleData
{
    public string assetBundleName;

    [field: SerializeField] [field: FormerlySerializedAs("keepLoaded")]
    public bool AlwaysKeepLoaded { get; private set; }

    public string configName;
    public List<WeatherData> weathers;
    public List<EnemyData> enemies;
    public List<ItemData> items;
    public List<MapObjectData> mapObjects;
    public List<UnlockableData> unlockables;
}