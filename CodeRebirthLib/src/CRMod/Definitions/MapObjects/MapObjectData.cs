using System;
using UnityEngine;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class MapObjectData : EntityData<CRMMapObjectReference>, IInspectorHeaderWarning
{
    public bool TryGetHeaderWarning(out string? message)
    {
        message = null;
        if (Key == null || string.IsNullOrEmpty(Key.ToString()) || Key.ToString() == ":")
        {
            message = "MapObject Data has no NamespacedKey.";
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
                        currentAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<CRMMapObjectDefinition>(path);
                    }
                }

                if (currentAsset != null && ((CRMMapObjectDefinition)currentAsset).GameObject == null)
                {
                    message = "MapObjectDefinition has no MapObject.";
                    return true;
                }
            }
        }
        return false;
    }

    public bool isInsideHazard;
    public bool createInsideHazardConfig;
    public string defaultInsideCurveSpawnWeights;
    public bool createInsideCurveSpawnWeightsConfig;
    public bool isOutsideHazard;
    public bool createOutsideHazardConfig;
    public string defaultOutsideCurveSpawnWeights;
    public bool createOutsideCurveSpawnWeightsConfig;
}