using UnityEngine;

namespace Dawn.Utils;
[AddComponentMenu($"{DawnConstants.MoonUtils}/Eclipsed Override")]
public class DawnEclipsedOverride : MonoBehaviour
{
    [field: SerializeField]
    public AudioClip? NewEclipsedMusic { get; private set; }

    private AudioClip _oldEclipsedMusic;
    private AudioSource _eclipsedAudioSource;

    private void Start()
    {
        GameObject effectObject = LethalContent.Weathers[WeatherKeys.Eclipsed].WeatherEffect.effectObject;

        _eclipsedAudioSource = effectObject.GetComponentInChildren<AudioSource>();
        _eclipsedAudioSource.clip = NewEclipsedMusic;
    }

    private void OnDestroy()
    {
        _eclipsedAudioSource.clip = _oldEclipsedMusic;
    }
}