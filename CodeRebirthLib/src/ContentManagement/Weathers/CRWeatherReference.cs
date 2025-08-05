using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Weathers;

[Serializable]
public class CRWeatherReference(string name) : CRContentReference<CRWeatherDefinition>(name)
{
    protected override string GetEntityName(CRWeatherDefinition obj) => obj.Weather.Name;

    public static implicit operator CRWeatherDefinition?(CRWeatherReference reference)
    {
        if (CRModWeatherExtensions.AllWeathers().TryGetFromWeatherName(reference.entityName, out var obj))
        {
            return obj;
        }
        return null;
    }
    
    public static implicit operator CRWeatherReference?(CRWeatherDefinition? obj)
    {
        if (obj) return new CRWeatherReference(obj!.Weather.Name);
        return null;
    }
}