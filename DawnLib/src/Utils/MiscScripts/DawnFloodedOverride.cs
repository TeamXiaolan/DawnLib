using UnityEngine;

namespace Dawn.Utils;

[AddComponentMenu($"{DawnConstants.MoonUtils}/Flooded Override")]
public class DawnFloodedOverride : MonoBehaviour
{
    [field: SerializeField]
    public AudioClip? NewFloodedAmbience { get; private set; }

    [field: SerializeField]
    public GameObject? NewFloodedPrefab { get; private set; }

    private AudioSource _floodedAmbienceSource;
    private AudioClip _oldFloodedAmbience;

    private GameObject _oldFloodedPrefab;
    private GameObject _newFloodInstance;

    public void Awake()
    {
        _oldFloodedPrefab = LethalContent.Weathers[WeatherKeys.Flooded].WeatherEffect.effectPermanentObject;
        if (NewFloodedAmbience != null)
        {
            _floodedAmbienceSource = _oldFloodedPrefab.GetComponentInChildren<AudioSource>();
            _oldFloodedAmbience = _floodedAmbienceSource.clip;
            _floodedAmbienceSource.clip = NewFloodedAmbience;
        }

        if (NewFloodedPrefab != null)
        {
            _newFloodInstance = Instantiate(NewFloodedPrefab, _oldFloodedPrefab.transform);
            _oldFloodedPrefab.transform.GetChild(0).gameObject.SetActive(false);
            _oldFloodedPrefab.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public void OnDestroy()
    {
        if (NewFloodedAmbience != null)
        {
            _floodedAmbienceSource.clip = _oldFloodedAmbience;
        }

        if (NewFloodedPrefab != null)
        {
            Destroy(_newFloodInstance);
            _oldFloodedPrefab.transform.GetChild(0).gameObject.SetActive(true);
            _oldFloodedPrefab.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}