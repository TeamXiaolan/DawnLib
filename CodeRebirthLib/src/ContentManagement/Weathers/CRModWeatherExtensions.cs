using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CodeRebirthLib.ContentManagement.Weathers;

public static class CRModWeatherExtensions
{
    public static CRRegistry<CRWeatherDefinition> WeatherRegistry(this CRMod mod)
    {
        return mod.GetRegistryByName<CRWeatherDefinition>(CRWeatherDefinition.REGISTRY_ID);
    }

    public static IEnumerable<CRWeatherDefinition> AllWeathers()
    {
        return CRMod.AllMods.SelectMany(mod => mod.WeatherRegistry());
    }

    public static bool TryGetFromWeatherName(this CRRegistry<CRWeatherDefinition> registry, string weatherName, [NotNullWhen(true)] out CRWeatherDefinition? value)
    {
        return registry.TryGetFirstBySomeName(it => it.Weather.Name, weatherName, out value);
    }

    /*public static bool TryGetDefinition(this Weather type, [NotNullWhen(true)] out CRWeatherDefinition? definition)
    {
        definition = AllWeathers().FirstOrDefault(it => it.Weather == type);
        return definition; // implict cast
    }*/ // todo?
}