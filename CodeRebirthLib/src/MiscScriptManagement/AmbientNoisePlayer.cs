using CodeRebirthLib.Extensions;
using CodeRebirthLib.Util;
using UnityEngine;
using UnityEngine.Events;

namespace CodeRebirthLib.MiscScriptManagement;
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
        _idleTimer = CodeRebirthLibNetworker.Instance!.CRLibRandom.NextFloat(_idleAudioClips.minTime, _idleAudioClips.maxTime);
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

        _idleTimer = CodeRebirthLibNetworker.Instance!.CRLibRandom.NextFloat(_idleAudioClips.minTime, _idleAudioClips.maxTime);
        _ambientAudioSource.PlayOneShot(_idleAudioClips.audioClips[CodeRebirthLibNetworker.Instance!.CRLibRandom.Next(_idleAudioClips.audioClips.Length)]);
        _onAmbientSoundPlayed.Invoke();
    }

    public void SetPlayable(bool isPlayable)
    {
        _canPlaySounds = isPlayable;
    }

    public void ResetAmbientTimer()
    {
        _idleTimer = CodeRebirthLibNetworker.Instance!.CRLibRandom.NextFloat(_idleAudioClips.minTime, _idleAudioClips.maxTime);
    }
    
    public void ForcePlayAmbientSound()
    {
        PlayRandomAmbientSound();
    }
}