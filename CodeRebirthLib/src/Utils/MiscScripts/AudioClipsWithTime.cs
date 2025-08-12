using System;
using UnityEngine;

namespace CodeRebirthLib;
[Serializable]
public class AudioClipsWithTime
{
    public AudioClip[] audioClips;
    public float minTime;
    public float maxTime;
}