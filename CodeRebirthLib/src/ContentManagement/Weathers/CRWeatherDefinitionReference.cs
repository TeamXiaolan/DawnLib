using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Weathers;

[Serializable]
public class CRWeatherDefinitionReference
{
    [SerializeField]
    private string weatherAsset;

    [SerializeField]
    private string weatherName;

    public string WeatherName => weatherName;

    public static implicit operator string?(CRWeatherDefinitionReference reference)
    {
        return reference.WeatherName;
    }

    public static implicit operator CRWeatherDefinition?(CRWeatherDefinitionReference reference)
    {
        if (CRModWeatherExtensions.AllWeathers().TryGetFromWeatherName(reference.weatherName, out var weather))
        {
            return weather;
        }
        return null;
    }
}