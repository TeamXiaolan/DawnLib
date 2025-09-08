using UnityEngine;

namespace Dawn;

public sealed class DawnSuitInfo
{
    public DawnUnlockableItemInfo ParentInfo { get; internal set; }

    internal DawnSuitInfo(Material suitMaterial, AudioClip? jumpAudioClip)
    {
        SuitMaterial = suitMaterial;
        JumpAudioClip = jumpAudioClip;
    }

    public Material SuitMaterial { get; }
    public AudioClip? JumpAudioClip { get; }
}