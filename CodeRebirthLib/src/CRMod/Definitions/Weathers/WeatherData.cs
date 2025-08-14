using System;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class WeatherData : EntityData<CRWeatherReference>
{
    public int spawnWeight;
    public float scrapMultiplier;
    public float scrapValueMultiplier;
    public bool isExclude;
    public bool createExcludeConfig;
    public string excludeOrIncludeList;
}