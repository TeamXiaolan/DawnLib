using System;
using UnityEngine;

namespace Dawn.Utils;
[Serializable]
public class AudioClipsWithTime
{
    public AudioClip[] audioClips;
    public float minTime;
    public float maxTime;
}