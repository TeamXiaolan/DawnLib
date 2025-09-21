using System.Collections.Generic;
using System.Linq;
using Dawn;
using UnityEngine;
using WeatherRegistry;
using WeatherRegistry.Modules;

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

        ImprovedWeatherEffect newImprovedWeatherEffect = new(weatherEffect);
        newImprovedWeatherEffect.SunAnimatorBool = SunAnimatorBool;
        newImprovedWeatherEffect.EffectObject?.SetActive(false);
        newImprovedWeatherEffect.WorldObject?.SetActive(false);

        Weather weather = new($"{WeatherName}", newImprovedWeatherEffect)
        {
            Color = TerminalColour,
            Config = new RegistryWeatherConfig
            {
                DefaultWeight = new IntegerConfigHandler(SpawnWeight),
                ScrapValueMultiplier = new FloatConfigHandler(ScrapValueMultiplier),
                ScrapAmountMultiplier = new FloatConfigHandler(ScrapAmountMultiplier),
                FilteringOption = new BooleanConfigHandler(!IsExclude, CreateExcludeConfig),
                LevelFilters = new LevelListConfigHandler(ExcludeOrIncludeList),
            }
        };

        WeatherManager.RegisterWeather(weather);
        HashSet<NamespacedKey> tags = _tags.ToHashSet();
        DawnWeatherEffectInfo weatherEffectInfo = new(TypedKey, tags, newImprovedWeatherEffect.VanillaWeatherEffect, null);
        newImprovedWeatherEffect.VanillaWeatherEffect.SetDawnInfo(weatherEffectInfo);
        LethalContent.Weathers.Register(weatherEffectInfo);
    }

    protected override string EntityNameReference => WeatherName;
}