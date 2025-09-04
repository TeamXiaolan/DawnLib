using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk;

[Serializable]
public class AssetBundleData
{
    [AssetBundleReference]
    public string assetBundleName;

    [field: SerializeField]
    [field: FormerlySerializedAs("keepLoaded")]
    public bool AlwaysKeepLoaded { get; private set; }

    [AssertNotEmpty]
    public string configName;
    public List<WeatherData> weathers;
    public List<EnemyData> enemies;
    public List<ItemData> items;
    public List<MapObjectData> mapObjects;
    public List<UnlockableData> unlockables;
    public List<DungeonData> dungeons;
}