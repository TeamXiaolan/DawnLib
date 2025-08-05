using System;

namespace CodeRebirthLib.ContentManagement.Items;
[Serializable]
public class ItemData : EntityData<CRItemReference>
{
    public string spawnWeights; // keep this here temporarily or somehow port stuff over from it?
    public bool generateSpawnWeightsConfig;
    public bool isScrap;
    public bool generateScrapConfig;
    public bool isShopItem;
    public bool generateShopItemConfig;
    public int cost;
}