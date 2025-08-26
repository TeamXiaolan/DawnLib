using System;
using UnityEngine;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class ItemData : EntityData<CRMItemReference>, IInspectorHeaderWarning
{
    public bool TryGetHeaderWarning(out string? message)
    {
        message = null;
        return false;
    }

    public string moonSpawnWeights;
    public string interiorSpawnWeights;
    public string weatherSpawnWeights;
    public bool generateSpawnWeightsConfig;
    public bool isScrap;
    public bool generateScrapConfig;
    public bool isShopItem;
    public bool generateShopItemConfig;
    public bool isProgressive;
    public bool generateProgressiveConfig;
    public int cost;
}