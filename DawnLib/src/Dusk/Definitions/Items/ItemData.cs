using System;
using UnityEngine.Serialization;

namespace Dawn.Dusk;
[Serializable]
public class ItemData : EntityData<CRMItemReference>
{
    public string moonSpawnWeights;
    public string interiorSpawnWeights;
    public string weatherSpawnWeights;
    public bool generateSpawnWeightsConfig;
    public bool isScrap;
    public bool generateScrapConfig;
    public bool isShopItem;
    public bool generateShopItemConfig;
    [FormerlySerializedAs("generateProgressiveConfig")] public bool generateDisableUnlockConfig;
    public int cost;
}