using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Items;
[Serializable]
public class ItemData : EntityData
{
    [SerializeReference]
    public CRItemReference itemReference = new(string.Empty);
    public override string EntityName => itemReference.entityName;

    public string spawnWeights;
    public bool generateSpawnWeightsConfig;
    public bool isScrap;
    public bool generateScrapConfig;
    public bool isShopItem;
    public bool generateShopItemConfig;
    public int cost;
}