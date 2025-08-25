using System;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class DungeonData : EntityData<CRMAdditionalTilesReference>, IInspectorHeaderWarning
{
    public bool TryGetHeaderWarning(out string? message)
    {
        message = null;
        if (Key == null || string.IsNullOrEmpty(Key.ToString()) || Key.ToString() == ":")
        {
            message = "Additional Tiles Data has no NamespacedKey.";
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
                        currentAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<CRMAdditionalTilesDefinition>(path);
                    }
                }

                if (currentAsset != null && ((CRMAdditionalTilesDefinition)currentAsset).TilesToAdd == null)
                {
                    message = "AdditionalTilesDefinition has no TileSet to add.";
                    return true;
                }
            }
        }
        return false;
    }
}