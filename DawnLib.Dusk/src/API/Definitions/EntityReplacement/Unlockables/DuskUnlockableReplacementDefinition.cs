using System.Collections;
using Dawn.Internal;
using Unity.Netcode;
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

        ApplyTyped((T)dawnUnlockable);
    }
}