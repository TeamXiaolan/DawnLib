using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using WeatherRegistry;
using WeatherRegistry.Modules;

namespace CodeRebirthLib.ContentManagement.Weathers;
[CreateAssetMenu(fileName = "New Weather Definition", menuName = "CodeRebirthLib/Definitions/Weather Definition")]
public class CRWeatherDefinition : CRContentDefinition<WeatherData>
{
    public const string REGISTRY_ID = "weathers";

    [field: FormerlySerializedAs("Weather")] [field: SerializeField]
    public Weather Weather { get; private set; }

    protected override string EntityNameReference => Weather.Name;

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

        Weather weather = new($"{Weather.Name}", newImprovedWeatherEffect);
        weather.Color = Weather.Color;
        weather.Config = new RegistryWeatherConfig
        {
            DefaultWeight = new IntegerConfigHandler(data.spawnWeight),
            ScrapValueMultiplier = new FloatConfigHandler(data.scrapValueMultiplier),
            ScrapAmountMultiplier = new FloatConfigHandler(data.scrapMultiplier),
            FilteringOption = new BooleanConfigHandler(!data.isExclude, data.createExcludeConfig),
            LevelFilters = new LevelListConfigHandler(data.excludeOrIncludeList),
        };

        this.Weather = weather;
        WeatherManager.RegisterWeather(weather);
        mod.WeatherRegistry().Register(this);
    }

    public static void RegisterTo(CRMod mod)
    {
        mod.CreateRegistry(REGISTRY_ID, new CRRegistry<CRWeatherDefinition>());
    }

    public override List<WeatherData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.weathers).ToList();
        // probably should be cached but i dont care anymore.
    }
}