using UnityEngine;

namespace Dawn;

public class DawnStingerDetail(AudioClip? firstTimeAudio, bool playsMoreThanOnce, float playChance, FuncProvider<bool> allowStingerToPlay)
{
    public AudioClip? FirstTimeAudio { get; private set; } = firstTimeAudio;
    public bool PlaysMoreThanOnce { get; private set; } = playsMoreThanOnce;
    public float PlayChance { get; private set; } = playChance;
    public FuncProvider<bool> AllowStingerToPlay { get; private set; } = allowStingerToPlay;
}