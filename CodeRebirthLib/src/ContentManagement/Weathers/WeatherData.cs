using System;

namespace CodeRebirthLib.ContentManagement.Weathers;
[Serializable]
public class WeatherData : EntityData
{
    public int spawnWeight;
    public float scrapMultiplier;
    public float scrapValueMultiplier;
    public bool isExclude;
    public bool createExcludeConfig;
    public string excludeOrIncludeList;
}