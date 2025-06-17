using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.AssetManagement;
using UnityEngine;
using UnityEngine.Serialization;
using WeatherRegistry;

namespace CodeRebirthLib.ContentManagement.Weathers;
[CreateAssetMenu(fileName = "New Weather Definition", menuName = "CodeRebirthLib/Definitions/Weather Definition")]
public class CRWeatherDefinition : CRContentDefinition<WeatherData>
{
    [field: FormerlySerializedAs("Weather"), SerializeField]
    public Weather Weather { get; private set; }

    public override void Register(CRMod mod, WeatherData data)
    {
        GameObject? effectObject = null;
        if (Weather.Effect.EffectObject != null)
        {
            effectObject = Instantiate(Weather.Effect.EffectObject);
            if (effectObject != null)
            {
                effectObject.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(effectObject);
            }
        }

        GameObject? effectPermanentObject = null;
        if (Weather.Effect.WorldObject != null)
        {
            effectPermanentObject = Instantiate(Weather.Effect.WorldObject);
            if (effectPermanentObject != null)
            {
                effectPermanentObject.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(effectPermanentObject);
            }
        }

        ImprovedWeatherEffect newImprovedWeatherEffect = new(effectObject, effectPermanentObject)
        {
            SunAnimatorBool = Weather.Effect.SunAnimatorBool,
        };

        Weather weather = new Weather($"{Weather.Name}", newImprovedWeatherEffect);
        weather.Color = Weather.Color;
        weather.Config = new()
        {
            DefaultWeight = new(data.spawnWeight),
            ScrapValueMultiplier = new(data.scrapValueMultiplier),
            ScrapAmountMultiplier = new(data.scrapMultiplier),
            FilteringOption = new(!data.isExclude, data.createExcludeConfig),
            LevelFilters = new(data.excludeOrIncludeList),
        };

        WeatherManager.RegisterWeather(weather);
        mod.WeatherRegistry().Register(this);
    }
    
    public const string REGISTRY_ID = "weathers";

    public static void RegisterTo(CRMod mod)
    {
        mod.CreateRegistry(REGISTRY_ID, new CRRegistry<CRWeatherDefinition>());
    }
    
    public override List<WeatherData> GetEntities(CRMod mod) => mod.Content.assetBundles.SelectMany(it => it.weathers).ToList(); // probably should be cached but i dont care anymore.
}