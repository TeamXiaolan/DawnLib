using System;
using UnityEngine;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class WeatherData : EntityData<CRMWeatherReference>, IInspectorHeaderWarning
{
    public bool TryGetHeaderWarning(out string? message)
    {
        message = null;
        if (Key == null || string.IsNullOrEmpty(Key.ToString()) || Key.ToString() == ":")
        {
            message = "Weather Data has no NamespacedKey.";
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
                        currentAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<CRMWeatherDefinition>(path);
                    }
                }

                if (currentAsset != null && ((CRMWeatherDefinition)currentAsset).Weather == null)
                {
                    message = "WeatherDefinition has no assigned Weather.";
                    return true;
                }
            }
        }
        return false;
    }

    public int spawnWeight;
    public float scrapMultiplier;
    public float scrapValueMultiplier;
    public bool isExclude;
    public bool createExcludeConfig;
    public string excludeOrIncludeList;
}