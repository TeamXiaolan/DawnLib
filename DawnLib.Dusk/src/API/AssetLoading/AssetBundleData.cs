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

    [Obsolete]
    private List<WeatherData> weathers;
    [Obsolete]
    private List<EnemyData> enemies;
    [Obsolete]
    private List<ItemData> items;
    [Obsolete]
    private List<MapObjectData> mapObjects;
    [Obsolete]
    private List<UnlockableData> unlockables;
    [Obsolete]
    private List<DungeonData> dungeons;
    [Obsolete]
    private List<VehicleData> vehicles;
}