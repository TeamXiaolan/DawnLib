using System.Collections;
using Dawn.Internal;
using UnityEngine;

namespace Dusk;

public class DuskUnlockableReplacementDefinition : DuskEntityReplacementDefinition<DuskUnlockable>
{
    public override IEnumerator Apply(DuskUnlockable ai, bool immediate = false)
    {
        yield break;
    }
}

public abstract class DuskUnlockableReplacementDefinition<T> : DuskUnlockableReplacementDefinition where T : DuskUnlockable
{
    protected abstract void ApplyTyped(T dawnUnlockable);
    public override IEnumerator Apply(DuskUnlockable dawnUnlockable, bool immediate = false)
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
            DuskPlugin.Logger.LogDebug($"Failed to apply replacement unlockable entity for '{dawnUnlockable.gameObject.name}', it doesn't have the right type!");
            yield break;
        }

        ApplyTyped((T)dawnUnlockable);
    }
}

public class DefaultUnlockableReplacementDefinition : DuskUnlockableReplacementDefinition<DuskUnlockable>
{
    protected override void ApplyTyped(DuskUnlockable dawnUnlockable) { }
}