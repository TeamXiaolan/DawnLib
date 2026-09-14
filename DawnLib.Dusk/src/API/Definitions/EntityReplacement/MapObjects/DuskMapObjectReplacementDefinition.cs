using System.Collections;
using Dawn.Internal;
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
    protected abstract void ApplyTyped(T duskMapObject);
    public override IEnumerator Apply(DuskMapObject duskMapObject, bool immediate = false)
    {
        Transform mapObjectTransform = duskMapObject.transform;
        duskMapObject.SetMapObjectReplacement(this);

        if (immediate)
        {
            StartOfRoundRefs.Instance.StartCoroutine(base.Apply(duskMapObject, immediate));
        }
        else
        {
            yield return StartOfRoundRefs.Instance.StartCoroutine(base.Apply(duskMapObject, immediate));
        }

        yield return StartOfRoundRefs.Instance.StartCoroutine(ApplyReplacementAndAddons(mapObjectTransform, immediate));

        if (duskMapObject == null)
        {
            yield break;
        }

        if (duskMapObject is not T)
        {
            DuskPlugin.Logger.LogDebug($"Failed to apply replacement map object entity for '{duskMapObject.gameObject.name}', it doesn't have the right type!");
            yield break;
        }

        ApplyTyped((T)duskMapObject);
    }
}

public class DefaultMapObjectReplacementDefinition : DuskMapObjectReplacementDefinition<DuskMapObject>
{
    protected override void ApplyTyped(DuskMapObject duskMapObject) { }
}