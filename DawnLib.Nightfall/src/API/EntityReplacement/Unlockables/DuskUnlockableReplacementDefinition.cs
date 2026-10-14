using System.Collections;
using Dawn.Internal;
using UnityEngine;

namespace Nightfall;

public class NightfallUnlockableReplacementDefinition : NightfallEntityReplacementDefinition<NightfallUnlockable>
{
    public override IEnumerator Apply(NightfallUnlockable ai, bool immediate = false)
    {
        yield break;
    }
}

public abstract class NightfallUnlockableReplacementDefinition<T> : NightfallUnlockableReplacementDefinition where T : NightfallUnlockable
{
    protected abstract void ApplyTyped(T dawnUnlockable);
    public override IEnumerator Apply(NightfallUnlockable dawnUnlockable, bool immediate = false)
    {
        Transform dawnUnlockableTransform = dawnUnlockable.transform;
        dawnUnlockable.SetUnlockableReplacement(this);

        if (immediate)
        {
            StartOfRoundRefs.Instance.StartCoroutine(base.Apply(dawnUnlockable, immediate));
        }
        else
        {
            yield return StartOfRoundRefs.Instance.StartCoroutine(base.Apply(dawnUnlockable, immediate));
        }

        yield return StartOfRoundRefs.Instance.StartCoroutine(ApplyReplacementAndAddons(dawnUnlockableTransform, immediate));

        if (dawnUnlockable == null)
        {
            yield break;
        }

        if (dawnUnlockable is not T)
        {
            NightfallPlugin.Logger.LogDebug($"Failed to apply replacement unlockable entity for '{dawnUnlockable.gameObject.name}', it doesn't have the right type!");
            yield break;
        }

        ApplyTyped((T)dawnUnlockable);
    }
}

public class DefaultUnlockableReplacementDefinition : NightfallUnlockableReplacementDefinition<NightfallUnlockable>
{
    protected override void ApplyTyped(NightfallUnlockable dawnUnlockable) { }
}