using System;
using Dawn.Internal;
using UnityEngine;

namespace Dawn;

[Serializable]
public class DawnTimeOfDayCues(AudioClip? dawnOverride, AudioClip? noonOverride, AudioClip? sundownOverride, AudioClip? midnightOverride)
{
    public static DawnTimeOfDayCues Default => new(null, null, null, null);

    [field: SerializeField]
    public AudioClip? DawnOverride { get; private set; } = dawnOverride;

    [field: SerializeField]
    public AudioClip? NoonOverride { get; private set; } = noonOverride;

    [field: SerializeField]
    public AudioClip? SundownOverride { get; private set; } = sundownOverride;

    [field: SerializeField]
    public AudioClip? MidnightOverride { get; private set; } = midnightOverride;

    public AudioClip GetClip(DayMode dayMode)
    {
        AudioClip? clip = dayMode switch
        {
            DayMode.Dawn => DawnOverride,
            DayMode.Noon => NoonOverride,
            DayMode.Sundown => SundownOverride,
            DayMode.Midnight => MidnightOverride,
            _ => throw new ArgumentOutOfRangeException(nameof(dayMode), dayMode, "Invalid DayMode value."),
        };

        if (clip == null)
        {
            return TimeOfDayRefs.Instance.timeOfDayCues[(int)dayMode];
        }

        return clip;
    }
}