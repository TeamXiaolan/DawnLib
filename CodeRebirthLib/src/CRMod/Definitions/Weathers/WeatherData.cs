using System;
using UnityEngine;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class WeatherData : EntityData<CRMWeatherReference>, IInspectorHeaderWarning
{
    public bool TryGetHeaderWarning(out string? message)
    {
        message = null;
        return false;
    }

    public int spawnWeight;
    public float scrapMultiplier;
    public float scrapValueMultiplier;
    public bool isExclude;
    public bool createExcludeConfig;
    public string excludeOrIncludeList;
}