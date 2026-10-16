using System.Collections;
using Dawn.Internal;
using UnityEngine;

namespace Nightfall;

public class NightfallMapObjectReplacementDefinition : NightfallEntityReplacementDefinition<NightfallMapObject>
{
    public override IEnumerator Apply(NightfallMapObject ai, bool immediate = false)
    {
        yield break;
    }
}

public abstract class NightfallMapObjectReplacementDefinition<T> : NightfallMapObjectReplacementDefinition where T : NightfallMapObject
{
    protected abstract void ApplyTyped(T nightfallMapObject);
    public override IEnumerator Apply(NightfallMapObject nightfallMapObject, bool immediate = false)
    {
        Transform mapObjectTransform = nightfallMapObject.transform;
        nightfallMapObject.SetMapObjectReplacement(this);

        if (immediate)
        {
            StartOfRoundRefs.Instance.StartCoroutine(base.Apply(nightfallMapObject, immediate));
        }
        else
        {
            yield return StartOfRoundRefs.Instance.StartCoroutine(base.Apply(nightfallMapObject, immediate));
        }

        yield return StartOfRoundRefs.Instance.StartCoroutine(ApplyReplacementAndAddons(mapObjectTransform, immediate));

        if (nightfallMapObject == null)
        {
            yield break;
        }

        if (nightfallMapObject is not T)
        {
            NightfallPlugin.Logger.LogDebug($"Failed to apply replacement map object entity for '{nightfallMapObject.gameObject.name}', it doesn't have the right type!");
            yield break;
        }

        ApplyTyped((T)nightfallMapObject);
    }
}

public class DefaultMapObjectReplacementDefinition : NightfallMapObjectReplacementDefinition<NightfallMapObject>
{
    protected override void ApplyTyped(NightfallMapObject nightfallMapObject) { }
}