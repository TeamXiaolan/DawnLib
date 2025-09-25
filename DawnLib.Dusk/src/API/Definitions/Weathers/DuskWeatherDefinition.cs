using System.Collections.Generic;
using System.Linq;
using Dawn;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Weather Definition", menuName = $"{DuskModConstants.Definitions}/Weather Definition")]
public class DuskWeatherDefinition : DuskContentDefinition<DawnWeatherEffectInfo>
{
    [field: SerializeField]
    public string WeatherName { get; private set; }
    [field: SerializeField]
    public GameObject TemporaryEffectObject { get; private set; }
    [field: SerializeField]
    public GameObject PermanentEffectObject { get; private set; }
    [field: SerializeField]
    public string SunAnimatorBool { get; private set; }
    [field: SerializeField]
    public Color TerminalColour { get; private set; }

    [field: Space(10)]
    [field: Header("Configs | Main")]
    [field: SerializeField]
    public int SpawnWeight { get; private set; }
    [field: SerializeField]
    public float ScrapValueMultiplier { get; private set; }
    [field: SerializeField]
    public float ScrapAmountMultiplier { get; private set; }

    [field: Header("Configs | Misc")]
    [field: SerializeField]
    public bool IsExclude { get; private set; }
    [field: SerializeField]
    public bool CreateExcludeConfig { get; private set; }
    [field: SerializeField]
    public string ExcludeOrIncludeList { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        GameObject? effectObject = null;
        if (TemporaryEffectObject != null)
        {
            effectObject = Instantiate(TemporaryEffectObject);
            if (effectObject != null)
            {
                effectObject.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(effectObject);
            }
        }

        GameObject? effectPermanentObject = null;
        if (PermanentEffectObject != null)
        {
            effectPermanentObject = Instantiate(PermanentEffectObject);
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

        WeatherRegistry.ImprovedWeatherEffect newImprovedWeatherEffect = new(weatherEffect);
        newImprovedWeatherEffect.SunAnimatorBool = SunAnimatorBool;
        newImprovedWeatherEffect.EffectObject?.SetActive(false);
        newImprovedWeatherEffect.WorldObject?.SetActive(false);

        WeatherRegistry.Weather weather = new($"{WeatherName}", newImprovedWeatherEffect)
        {
            Color = TerminalColour,
            Config = new WeatherRegistry.Modules.RegistryWeatherConfig
            {
                DefaultWeight = new WeatherRegistry.IntegerConfigHandler(SpawnWeight),
                ScrapValueMultiplier = new WeatherRegistry.FloatConfigHandler(ScrapValueMultiplier),
                ScrapAmountMultiplier = new WeatherRegistry.FloatConfigHandler(ScrapAmountMultiplier),
                FilteringOption = new WeatherRegistry.BooleanConfigHandler(!IsExclude, CreateExcludeConfig),
                LevelFilters = new WeatherRegistry.LevelListConfigHandler(ExcludeOrIncludeList),
            }
        };

        WeatherRegistry.WeatherManager.RegisterWeather(weather);
        HashSet<NamespacedKey> tags = _tags.ToHashSet();
        DawnWeatherEffectInfo weatherEffectInfo = new(TypedKey, tags, newImprovedWeatherEffect.VanillaWeatherEffect, null);
        newImprovedWeatherEffect.VanillaWeatherEffect.SetDawnInfo(weatherEffectInfo);
        LethalContent.Weathers.Register(weatherEffectInfo);
    }

    protected override string EntityNameReference => WeatherName;
}