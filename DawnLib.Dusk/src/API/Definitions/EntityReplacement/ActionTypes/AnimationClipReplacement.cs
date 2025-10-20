using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New AnimationClip Replacement", menuName = $"Entity Replacements/Actions/AnimationClip Replacement")]
public class AnimationClipReplacement : Hierarchy
{
    [field: SerializeField]
    public string OriginalClipName { get; private set; }
    [field: SerializeField]
    public AnimationClip NewAnimationClip { get; private set; }
    [field: SerializeField]
    public List<AnimationEventAddition> PotentialAnimationEvents { get; private set; } = new();

    public override IEnumerator Apply(Transform rootTransform)
    {
        yield return null;
        Animator animator = !string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).GetComponent<Animator>() : rootTransform.GetComponent<Animator>();
        AnimatorOverrideController animatorOverrideController = new(animator.runtimeAnimatorController);
        foreach (AnimationEventAddition animationEventAddition in PotentialAnimationEvents)
        {
            AnimationEvent animationEvent = new()
            {
                functionName = animationEventAddition.AnimationEventName,
                time = animationEventAddition.Time,

                stringParameter = animationEventAddition.StringParameter,
                intParameter = animationEventAddition.IntParameter,
                floatParameter = animationEventAddition.FloatParameter,
                objectReferenceParameter = animationEventAddition.ObjectParameter
            };

            NewAnimationClip.AddEvent(animationEvent);
        }
        animatorOverrideController[OriginalClipName] = NewAnimationClip;
        animator.runtimeAnimatorController = animatorOverrideController;
    }
}

[Serializable]
public class AnimationEventAddition
{
    [field: SerializeField]
    public string AnimationEventName { get; private set; }
    [field: SerializeField]
    public float Time { get; private set; }

    [field: Header("Optional | Parameters")]
    [field: SerializeField]
    public string StringParameter { get; private set; } = string.Empty;
    [field: SerializeField]
    public int IntParameter { get; private set; } = 0;
    [field: SerializeField]
    public float FloatParameter { get; private set; } = 0f;
    [field: SerializeField]
    public bool BoolParameter { get; private set; } = false;
    [field: SerializeField]
    public UnityEngine.Object? ObjectParameter { get; private set; } = null;
}