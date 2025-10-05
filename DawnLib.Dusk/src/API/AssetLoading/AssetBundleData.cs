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
    [SerializeField]
    [HideInInspector]
    private List<WeatherData> weathers;
    [Obsolete]
    [SerializeField]
    [HideInInspector]
    private List<EnemyData> enemies;
    [Obsolete]
    [SerializeField]
    [HideInInspector]
    private List<ItemData> items;
    [Obsolete]
    [SerializeField]
    [HideInInspector]
    private List<MapObjectData> mapObjects;
    [Obsolete]
    [SerializeField]
    [HideInInspector]
    private List<UnlockableData> unlockables;
    [Obsolete]
    [SerializeField]
    [HideInInspector]
    private List<AdditionalTilesData> dungeons;
    [Obsolete]
    [SerializeField]
    [HideInInspector]
    private List<VehicleData> vehicles;
}