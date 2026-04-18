using System.Collections;
using Dawn.Internal;
using UnityEngine;

namespace Dusk;

public class DuskItemReplacementDefinition : DuskEntityReplacementDefinition<GrabbableObject>
{
    [field: SerializeField]
    public bool IsConductiveMetal { get; private set; }

    [field: SerializeField]
    public AudioClip? GrabSFX { get; private set; }

    [field: SerializeField]
    public AudioClip? DropSFX { get; private set; }

    [field: SerializeField]
    public AudioClip? PocketSFX { get; private set; }

    [field: SerializeField]
    public AudioClip? ThrowSFX { get; private set; }

    [field: SerializeField]
    public float VerticalOffset { get; private set; }

    [field: SerializeField]
    public int FloorYOffset { get; private set; }

    [field: SerializeField]
    public Vector3 RestingRotation { get; private set; } = new Vector3(0f, 0f, 90f);

    [field: SerializeField]
    public Vector3 RotationOffset { get; private set; }

    [field: SerializeField]
    public Vector3 PositionOffset { get; private set; }

    public override IEnumerator Apply(GrabbableObject ai, bool immediate = false)
    {
        yield break;
    }
}

public abstract class DuskItemReplacementDefinition<T> : DuskItemReplacementDefinition where T : GrabbableObject
{
    protected abstract void ApplyTyped(T grabbableObject);
    public override IEnumerator Apply(GrabbableObject grabbableObject, bool immediate = false)
    {
        Transform grabbableTransform = grabbableObject.transform;
        grabbableObject.SetGrabbableObjectReplacement(this);

        if (immediate)
        {
            StartOfRoundRefs.Instance.StartCoroutine(base.Apply(grabbableObject, immediate));
        }
        else
        {
            yield return StartOfRoundRefs.Instance.StartCoroutine(base.Apply(grabbableObject, immediate));
        }

        yield return StartOfRoundRefs.Instance.StartCoroutine(ApplyReplacementAndAddons(grabbableTransform, immediate));

        if (grabbableObject == null)
        {
            yield break;
        }

        ApplyTyped((T)grabbableObject);
    }
}