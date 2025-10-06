using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Dusk;

public class TransferAnimationClips : MonoBehaviour
{
    public List<AnimationClipReplacement> AnimationClipReplacements { get; internal set; } = new();

    private void Start()
    {
        if (AnimationClipReplacements.Count <= 0)
        {
            DuskPlugin.Logger.LogError("TransferAnimationClips: No AnimationClipReplacements.");
            return;
        }

        RuntimeAnimatorController? runtimeAnimatorController = GetComponent<Animator>()?.runtimeAnimatorController;
        if (runtimeAnimatorController == null)
        {
            DuskPlugin.Logger.LogError($"TransferAnimationClips: Target {this.gameObject} has no Animator or RuntimeAnimatorController.");
            return;
        }

        AnimatorOverrideController animatorOverrideController = new(runtimeAnimatorController);
    }

    private static void ReplaceAnimationClip(ParticleSystem particleSystem, ParticleSystem targetParticleSystem)
    {
        // todo
    }
}