using System;
using UnityEngine;

namespace CodeRebirthLib;

[Serializable]
public class AudioSourceWithClips
{
    [Tooltip("Near AudioSource that will play the clips.")]
    public AudioSource nearAudioSource = null!;

    [Tooltip("List of close audio clips that can be played.")]
    public AudioClip[] nearClips = null!;

    [Tooltip("Far AudioSource that will play the clips.")]
    public AudioSource farAudioSource = null!;

    [Tooltip("List of far audio clips that can be played.")]
    public AudioClip[] farClips = null!;
}