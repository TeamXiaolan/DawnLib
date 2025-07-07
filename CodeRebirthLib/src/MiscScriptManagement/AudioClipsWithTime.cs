using System;
using UnityEngine;

namespace CodeRebirthLib.MiscScriptManagement;
[Serializable]
public class AudioClipsWithTime
{
    public AudioClip[] audioClips;
    public float minTime;
    public float maxTime;
}