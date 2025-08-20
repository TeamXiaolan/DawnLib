using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class AssetBundleData
{
    public string assetBundleName;

    [field: SerializeField]
    [field: FormerlySerializedAs("keepLoaded")]
    public bool AlwaysKeepLoaded { get; private set; }

    public string configName;
    public List<WeatherData> weathers;
    public List<EnemyData> enemies;
    public List<ItemData> items;
    public List<MapObjectData> mapObjects;
    public List<UnlockableData> unlockables;
    public List<AchievementData> achievements = new();
    public List<DungeonData> dungeons = new();
}