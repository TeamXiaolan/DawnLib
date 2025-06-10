namespace CodeRebirthLib.ContentManagement.Weathers;
public static class CRModWeatherExtensions
{
    public static CRRegistry<CRWeatherDefinition> WeatherRegistry(this CRMod mod)
    {
        return mod.GetRegistryByName<CRWeatherDefinition>(CRWeatherDefinition.REGISTRY_ID);
    }
}