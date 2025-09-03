using System;

namespace Dawn.Dusk;
[Serializable]
public class WeatherData : EntityData<DuskWeatherReference>
{
    public int spawnWeight;
    public float scrapMultiplier;
    public float scrapValueMultiplier;
    public bool isExclude;
    public bool createExcludeConfig;
    public string excludeOrIncludeList;
}