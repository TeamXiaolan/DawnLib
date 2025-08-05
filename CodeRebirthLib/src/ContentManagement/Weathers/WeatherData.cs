using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Weathers;
[Serializable]
public class WeatherData : EntityData
{
    [SerializeReference]
    public CRWeatherReference weatherReference = new(string.Empty);
    public override string EntityName => weatherReference.entityName;

    public int spawnWeight;
    public float scrapMultiplier;
    public float scrapValueMultiplier;
    public bool isExclude;
    public bool createExcludeConfig;
    public string excludeOrIncludeList;
}