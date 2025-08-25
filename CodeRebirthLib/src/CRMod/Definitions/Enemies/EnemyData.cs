using System;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class EnemyData : EntityData<CRMEnemyReference>, IInspectorHeaderWarning
{
    public bool TryGetHeaderWarning(out string? message)
    {
        message = null;
        if (Key == null || string.IsNullOrEmpty(Key.ToString()) || Key.ToString() == ":")
        {
            message = "Enemy Data has no NamespacedKey.";
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
                        currentAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<CRMEnemyDefinition>(path);
                    }
                }

                if (currentAsset != null && ((CRMEnemyDefinition)currentAsset).EnemyType == null)
                {
                    message = "EnemyDefinition has no EnemyType.";
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
    public float powerLevel;
    public int maxSpawnCount;
}