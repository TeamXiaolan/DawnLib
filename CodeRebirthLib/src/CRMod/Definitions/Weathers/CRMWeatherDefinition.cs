using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeatherRegistry;
using WeatherRegistry.Modules;

namespace CodeRebirthLib.CRMod;
[CreateAssetMenu(fileName = "New Weather Definition", menuName = "CodeRebirthLib/Definitions/Weather Definition")]
public class CRMWeatherDefinition : CRMContentDefinition<WeatherData, CRWeatherInfo>
{
    public const string REGISTRY_ID = "weathers";

    [field: SerializeField]
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

        WeatherEffect weatherEffect = new();
        {
            effectObject = Weather.Effect.EffectObject;
            effectPermanentObject = Weather.Effect.WorldObject;
        }
        ImprovedWeatherEffect newImprovedWeatherEffect = new(weatherEffect)
        {
            SunAnimatorBool = Weather.Effect.SunAnimatorBool,
        };

        Weather weather = new($"{Weather.Name}", newImprovedWeatherEffect)
        {
            Color = Weather.Color,
            Config = new RegistryWeatherConfig
            {
                DefaultWeight = new IntegerConfigHandler(data.spawnWeight),
                ScrapValueMultiplier = new FloatConfigHandler(data.scrapValueMultiplier),
                ScrapAmountMultiplier = new FloatConfigHandler(data.scrapMultiplier),
                FilteringOption = new BooleanConfigHandler(!data.isExclude, data.createExcludeConfig),
                LevelFilters = new LevelListConfigHandler(data.excludeOrIncludeList),
            }
        };

        this.Weather = weather;
        WeatherManager.RegisterWeather(weather);
        /*CRLib.DefineWeather(TypedKey, weather.Effect.VanillaWeatherEffect, builder =>
        {
            builder.Build();
        });*/
    }

    public override List<WeatherData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.weathers).ToList();
        // probably should be cached but i dont care anymore.
    }

    public override string GetDefaultKey()
    {
        if (Weather == null)
        {
            return "";
        }
        return Weather.name;
    }
}