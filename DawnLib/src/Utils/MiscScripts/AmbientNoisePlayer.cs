using UnityEngine;
using UnityEngine.Events;

namespace Dawn.Utils;
[AddComponentMenu($"{DawnConstants.AudioComponents}/Ambient Noise Player")]
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
    private System.Random random;

    private static int instances = 0;

    public void Start()
    {
        random = new System.Random(StartOfRound.Instance.randomMapSeed + instances);
        instances++;
        _idleTimer = random.NextFloat(_idleAudioClips.minTime, _idleAudioClips.maxTime);
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

        _idleTimer = random.NextFloat(_idleAudioClips.minTime, _idleAudioClips.maxTime);
        _ambientAudioSource.PlayOneShot(_idleAudioClips.audioClips[random.Next(_idleAudioClips.audioClips.Length)]);
        _onAmbientSoundPlayed.Invoke();
    }

    public void SetPlayable(bool isPlayable)
    {
        _canPlaySounds = isPlayable;
    }

    public void ResetAmbientTimer()
    {
        _idleTimer = random.NextFloat(_idleAudioClips.minTime, _idleAudioClips.maxTime);
    }

    public void ForcePlayAmbientSound()
    {
        PlayRandomAmbientSound();
    }
}