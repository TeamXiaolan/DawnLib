using System.Collections;
using System.Collections.Generic;
using Dawn.Utils;
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
    public List<AnimationEventData> PotentialAnimationEvents { get; private set; } = new();

    public override IEnumerator Apply(Transform rootTransform, bool immediate = false)
    {
        if (!immediate)
        {
            yield return null;
        }

        Animator animator = !string.IsNullOrWhiteSpace(HierarchyPath) ? rootTransform.Find(HierarchyPath).GetComponent<Animator>() : rootTransform.GetComponent<Animator>();
        AnimatorOverrideController animatorOverrideController = new(animator.runtimeAnimatorController);
        foreach (AnimationEventData animationEventAddition in PotentialAnimationEvents)
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