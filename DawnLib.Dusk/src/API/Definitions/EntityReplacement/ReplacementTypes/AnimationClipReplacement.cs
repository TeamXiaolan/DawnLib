using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dusk;

[Serializable]
public class AnimationClipReplacement
{
    [field: SerializeField]
    public AnimationClip NewAnimationClip { get; private set; }
    [field: SerializeField]
    public List<AnimationEventAddition> PotentialAnimationEvents { get; private set; } = new();
}