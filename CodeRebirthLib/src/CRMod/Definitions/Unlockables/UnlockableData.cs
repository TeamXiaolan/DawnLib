using System;
using UnityEngine;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class UnlockableData : EntityData<CRMUnlockableReference>, IInspectorHeaderWarning
{
    public bool TryGetHeaderWarning(out string? message)
    {
        message = null;
        if (Key == null || string.IsNullOrEmpty(Key.ToString()) || Key.ToString() == ":")
        {
            message = "UnlockableItem Data has no NamespacedKey.";
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
                        currentAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<CRMUnlockableDefinition>(path);
                    }
                }

                if (currentAsset != null && string.IsNullOrEmpty(((CRMUnlockableDefinition)currentAsset).UnlockableItem.unlockableName))
                {
                    message = "UnlockableDefinition has no name assigned to it.";
                    return true;
                }
            }
        }
        return false;
    }

    public int cost;
    public bool isShipUpgrade;
    public bool isDecor;
    public bool isProgressive;
    public bool createProgressiveConfig;
}