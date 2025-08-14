using System;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class WeatherData : EntityData<CRMWeatherReference>
{
    public int spawnWeight;
    public float scrapMultiplier;
    public float scrapValueMultiplier;
    public bool isExclude;
    public bool createExcludeConfig;
    public string excludeOrIncludeList;
}