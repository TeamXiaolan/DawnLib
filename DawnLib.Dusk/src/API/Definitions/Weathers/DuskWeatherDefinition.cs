using System.Collections.Generic;
using System.Linq;
using Dawn;
using UnityEngine;
using WeatherRegistry;
using WeatherRegistry.Modules;

namespace Dusk;

[CreateAssetMenu(fileName = "New Weather Definition", menuName = $"{DuskModConstants.Definitions}/Weather Definition")]
public class DuskWeatherDefinition : DuskContentDefinition<WeatherData, DawnWeatherEffectInfo>
{
    public const string REGISTRY_ID = "weathers";

    [field: SerializeField]
    public Weather Weather { get; private set; }

    public override void Register(DuskMod mod, WeatherData data)
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

        WeatherEffect weatherEffect = new()
        {
            effectObject = effectObject,
            effectPermanentObject = effectPermanentObject,
        };

        ImprovedWeatherEffect newImprovedWeatherEffect = new(weatherEffect);
        newImprovedWeatherEffect.SunAnimatorBool = Weather.Effect.SunAnimatorBool;
        newImprovedWeatherEffect.EffectObject?.SetActive(false);
        newImprovedWeatherEffect.WorldObject?.SetActive(false);

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
        HashSet<NamespacedKey> tags = _tags.ToHashSet();
        DawnWeatherEffectInfo weatherEffectInfo = new(TypedKey, tags, newImprovedWeatherEffect.VanillaWeatherEffect, null);
        newImprovedWeatherEffect.VanillaWeatherEffect.SetDawnInfo(weatherEffectInfo);
        LethalContent.Weathers.Register(weatherEffectInfo);
    }

    public override List<WeatherData> GetEntities(DuskMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.weathers).ToList();
        // probably should be cached but i dont care anymore.
    }

    protected override string EntityNameReference => Weather?.Name ?? string.Empty;
}