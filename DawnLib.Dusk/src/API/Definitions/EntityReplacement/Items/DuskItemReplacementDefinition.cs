using System.Collections;
using Dawn.Internal;
using UnityEngine;

namespace Dusk;

public class DuskItemReplacementDefinition : DuskEntityReplacementDefinition<GrabbableObject>
{
    [field: SerializeField]
    public Sprite ItemIcon { get; private set; }

    [field: SerializeField]
    public string DisplayName { get; private set; }

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

    [field: SerializeField]
    public string[] ToolTips { get; private set; }

    internal override void RegisterAsDefault(GrabbableObject grabbableObject, string baseKey, string @namespace, string variantKey)
    {
        VerticalOffset = grabbableObject.itemProperties.verticalOffset;
        FloorYOffset = grabbableObject.itemProperties.floorYOffset;
        RestingRotation = grabbableObject.itemProperties.restingRotation;
        RotationOffset = grabbableObject.itemProperties.rotationOffset;
        PositionOffset = grabbableObject.itemProperties.positionOffset;
        ToolTips = grabbableObject.itemProperties.toolTips;
        base.RegisterAsDefault(grabbableObject, baseKey, @namespace, variantKey);
    }

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

        if (grabbableObject is not T)
        {
            DuskPlugin.Logger.LogDebug($"Failed to apply replacement item entity for '{grabbableObject.itemProperties.itemName}', it doesn't have the right type!");
            yield break;
        }

        ApplyTyped((T)grabbableObject);
    }
}

public class DefaultItemReplacementDefinition : DuskItemReplacementDefinition<PhysicsProp>
{
    protected override void ApplyTyped(PhysicsProp physicsProp) { }
}