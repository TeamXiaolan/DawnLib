using UnityEngine;

namespace Dawn.Utils;

public class DawnStormyOverride : MonoBehaviour
{
    [field: SerializeField]
    public AudioClip? NewStormyRainAmbience { get; private set; }

    [field: SerializeField]
    public ParticleSystem? NewStormyLightningStrikeExplosionPrefab { get; private set; }

    [field: SerializeField]
    public ParticleSystem? NewStormyStaticElectricityPrefab { get; private set; }

    [field: SerializeField]
    public AudioClip[] NewLightningStrikeSFX { get; private set; }

    [field: SerializeField]
    public AudioClip[] NewDistantThunderSFX { get; private set; }

    [field: SerializeField]
    public AudioClip NewStaticElectricitySFX { get; private set; }

    [field: SerializeField]
    public GameObject? NewStormyRainPrefab { get; private set; }

    private GameObject _stormyObject;
    private AudioSource _stormyAmbienceSource;

    private AudioClip _oldStormyAmbience;

    private GameObject _newStormyRainInstance;
    private ParticleSystem _oldExplosionEffectParticle;
    private ParticleSystem _oldStaticElectricityParticle;

    private AudioClip[] _oldLightningStrikeSFX;
    private AudioClip[] _oldDistantThunderSFX;
    private AudioClip _oldStaticElectricitySFX;

    public void Awake()
    {
        _stormyObject = LethalContent.Weathers[WeatherKeys.Flooded].WeatherEffect.effectPermanentObject;
        if (NewStormyRainAmbience != null)
        {
            _stormyAmbienceSource = _stormyObject.GetComponentInChildren<AudioSource>();
            _oldStormyAmbience = _stormyAmbienceSource.clip;
            _stormyAmbienceSource.clip = NewStormyRainAmbience;
        }

        StormyWeather stormyWeather = _stormyObject.GetComponent<StormyWeather>();
        if (NewStormyLightningStrikeExplosionPrefab != null)
        {
            _oldExplosionEffectParticle = stormyWeather.explosionEffectParticle;
            stormyWeather.explosionEffectParticle = GameObject.Instantiate(NewStormyLightningStrikeExplosionPrefab, _stormyObject.transform);
        }

        if (NewStormyStaticElectricityPrefab != null)
        {
            _oldStaticElectricityParticle = stormyWeather.staticElectricityParticle;
            stormyWeather.staticElectricityParticle = GameObject.Instantiate(NewStormyStaticElectricityPrefab, _stormyObject.transform);
        }

        if (NewLightningStrikeSFX.Length > 0)
        {
            _oldLightningStrikeSFX = stormyWeather.strikeSFX;
            stormyWeather.strikeSFX = NewLightningStrikeSFX;
        }

        if (NewDistantThunderSFX.Length > 0)
        {
            _oldDistantThunderSFX = stormyWeather.distantThunderSFX;
            stormyWeather.distantThunderSFX = NewDistantThunderSFX;
        }

        if (NewStaticElectricitySFX != null)
        {
            _oldStaticElectricitySFX = stormyWeather.staticElectricityAudio;
            stormyWeather.staticElectricityAudio = NewStaticElectricitySFX;
        }

        if (NewStormyRainPrefab != null)
        {
            GameObject oldStormyRainPrefab = LethalContent.Weathers[WeatherKeys.Flooded].WeatherEffect.effectObject.transform.Find("Particle System").gameObject;
            _newStormyRainInstance = GameObject.Instantiate(NewStormyRainPrefab, oldStormyRainPrefab.transform.parent);
            oldStormyRainPrefab.SetActive(false);
        }
    }

    public void OnDestroy()
    {
        if (NewStormyRainAmbience != null)
        {
            _stormyAmbienceSource.clip = _oldStormyAmbience;
        }

        StormyWeather stormyWeather = _stormyObject.GetComponent<StormyWeather>();
        if (NewStormyLightningStrikeExplosionPrefab != null)
        {
            GameObject.Destroy(stormyWeather.explosionEffectParticle);
            stormyWeather.explosionEffectParticle = _oldExplosionEffectParticle;
        }

        if (NewStormyStaticElectricityPrefab != null)
        {
            GameObject.Destroy(stormyWeather.staticElectricityParticle);
            stormyWeather.staticElectricityParticle = _oldStaticElectricityParticle;
        }

        if (NewLightningStrikeSFX.Length > 0)
        {
            stormyWeather.strikeSFX = _oldLightningStrikeSFX;
        }

        if (NewDistantThunderSFX.Length > 0)
        {
            stormyWeather.distantThunderSFX = _oldDistantThunderSFX;
        }

        if (NewStaticElectricitySFX != null)
        {
            stormyWeather.staticElectricityAudio = _oldStaticElectricitySFX;
        }

        if (NewStormyRainPrefab != null)
        {
            GameObject oldStormyRainPrefab = LethalContent.Weathers[WeatherKeys.Flooded].WeatherEffect.effectObject;
            Destroy(_newStormyRainInstance);
            oldStormyRainPrefab.SetActive(true);
        }
    }
}