using System.Collections;
using Dawn.Internal;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

public class DuskMapObjectReplacementDefinition : DuskEntityReplacementDefinition<DuskMapObject>
{
    public override IEnumerator Apply(DuskMapObject ai, bool immediate = false)
    {
        yield break;
    }
}

public abstract class DuskMapObjectReplacementDefinition<T> : DuskMapObjectReplacementDefinition where T : DuskMapObject
{
    protected abstract void ApplyTyped(T dawnMapObject);
    public override IEnumerator Apply(DuskMapObject dawnMapObject, bool immediate = false)
    {
        Transform mapObjectTransform = dawnMapObject.transform;
        dawnMapObject.SetMapObjectReplacement(this);

        if (immediate)
        {
            StartOfRoundRefs.Instance.StartCoroutine(base.Apply(dawnMapObject, immediate));
        }
        else
        {
            yield return StartOfRoundRefs.Instance.StartCoroutine(base.Apply(dawnMapObject, immediate));
        }

        yield return StartOfRoundRefs.Instance.StartCoroutine(ApplyReplacementAndAddons(mapObjectTransform, immediate));

        if (dawnMapObject == null)
        {
            yield break;
        }

        ApplyTyped((T)dawnMapObject);
    }
}