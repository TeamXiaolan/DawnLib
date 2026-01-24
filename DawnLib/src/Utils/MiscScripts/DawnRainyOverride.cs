using UnityEngine;

namespace Dawn.Utils;

public class DawnRainyOverride : MonoBehaviour
{
    [field: SerializeField]
    public AudioClip? NewRainSound { get; private set; } = null;
    [field: SerializeField]
    public GameObject? NewRainyPrefab { get; private set; }

    private AudioClip? _oldRainSound;
    private AudioSource _rainAudioSource;

    private GameObject _rainyOverridePrefab;
    private GameObject _rainyParticlesPrefab;

    public void Awake()
    {
        GameObject effectObject = LethalContent.Weathers[WeatherKeys.Rainy].WeatherEffect.effectObject;

        if (NewRainSound != null)
        {
            _rainAudioSource = effectObject.GetComponent<AudioSource>();

            _oldRainSound = _rainAudioSource.clip;
            _rainAudioSource.clip = NewRainSound;
        }

        if (NewRainyPrefab != null)
        {
            _rainyParticlesPrefab = effectObject.transform.Find("Particle System").gameObject;
            _rainyParticlesPrefab.SetActive(false);

            _rainyOverridePrefab = GameObject.Instantiate(NewRainyPrefab, effectObject.transform);
        }
    }

    public void OnDestroy()
    {
        if (NewRainSound != null)
        {
            _rainAudioSource.clip = _oldRainSound;
        }

        if (NewRainyPrefab != null)
        {
            GameObject.Destroy(_rainyOverridePrefab);
            _rainyParticlesPrefab.SetActive(true);
        }
    }
}