using System;
using UnityEngine;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class ItemData : EntityData<CRMItemReference>, IInspectorHeaderWarning
{
    public bool TryGetHeaderWarning(out string? message)
    {
        message = null;
        if (Key == null || string.IsNullOrEmpty(Key.ToString()) || Key.ToString() == ":")
        {
            message = "Item Data has no NamespacedKey.";
            return true;
        }

        if (Application.isEditor)
        {
            if (Reference != null)
            {
                object? currentAsset = null;
                if (!string.IsNullOrEmpty(Reference.assetGUID))
                {
                    string guid = Reference.assetGUID;
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                    if (!string.IsNullOrEmpty(path))
                    {
                        currentAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<CRMItemDefinition>(path);
                    }
                }

                if (currentAsset != null && ((CRMItemDefinition)currentAsset).Item == null)
                {
                    message = "ItemDefinition has no Item.";
                    return true;
                }
            }
        }
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