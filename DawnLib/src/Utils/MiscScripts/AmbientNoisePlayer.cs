using Dawn.Internal;
using UnityEngine;
using UnityEngine.Events;

namespace Dawn.Utils;
public class AmbientNoisePlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField]
    private AudioSource _ambientAudioSource = null!;
    [SerializeField]
    private AudioClipsWithTime _idleAudioClips = null!;

    [Header("Extras")]
    [SerializeField]
    private bool _playOnStart = false;
    [SerializeField]
    private UnityEvent _onAmbientSoundPlayed = new();

    private bool _canPlaySounds = true;
    private float _idleTimer = 0f;

    public void Start()
    {
        _idleTimer = DawnNetworker.Instance!.DawnLibRandom.NextFloat(_idleAudioClips.minTime, _idleAudioClips.maxTime);
        if (!_playOnStart)
            return;

        PlayRandomAmbientSound();
    }

    public void Update()
    {
        if (!_canPlaySounds)
            return;

        _idleTimer -= Time.deltaTime;
        if (_idleTimer > 0)
            return;

        PlayRandomAmbientSound();
    }

    private void PlayRandomAmbientSound()
    {
        if (_idleAudioClips.audioClips.Length <= 0)
            return;

        _idleTimer = DawnNetworker.Instance!.DawnLibRandom.NextFloat(_idleAudioClips.minTime, _idleAudioClips.maxTime);
        _ambientAudioSource.PlayOneShot(_idleAudioClips.audioClips[DawnNetworker.Instance!.DawnLibRandom.Next(_idleAudioClips.audioClips.Length)]);
        _onAmbientSoundPlayed.Invoke();
    }

    public void SetPlayable(bool isPlayable)
    {
        _canPlaySounds = isPlayable;
    }

    public void ResetAmbientTimer()
    {
        _idleTimer = DawnNetworker.Instance!.DawnLibRandom.NextFloat(_idleAudioClips.minTime, _idleAudioClips.maxTime);
    }

    public void ForcePlayAmbientSound()
    {
        PlayRandomAmbientSound();
    }
}