using System.Collections;
using Dawn.Internal;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

public class DuskItemReplacementDefinition : DuskEntityReplacementDefinition<GrabbableObject>
{
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
        // yield return base.Apply(grabbableObject);
        if (grabbableObject != null)
        {
            grabbableObject.SetGrabbableObjectReplacement(this);
        }

        foreach (Hierarchy hierarchyReplacement in Replacements)
        {
            if (immediate)
            {
                StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(grabbableTransform, immediate));
            }
            else
            {
                yield return StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(grabbableTransform, immediate));
            }
        }

        foreach (GameObjectWithPath gameObjectAddon in GameObjectAddons)
        {
            GameObject gameObject = !string.IsNullOrWhiteSpace(gameObjectAddon.PathToGameObject) ? grabbableTransform.Find(gameObjectAddon.PathToGameObject).gameObject : grabbableTransform.gameObject;
            if (gameObjectAddon.GameObjectToCreate.TryGetComponent(out NetworkObject networkObject) && !NetworkManager.Singleton.IsServer)
                continue;

            GameObject addOn = GameObject.Instantiate(gameObjectAddon.GameObjectToCreate, gameObject.transform);
            addOn.transform.SetLocalPositionAndRotation(gameObjectAddon.PositionOffset, Quaternion.Euler(gameObjectAddon.RotationOffset));
            if (networkObject == null)
                continue;

            networkObject.AutoObjectParentSync = false;
            networkObject.Spawn();
        }

        if (grabbableObject == null)
        {
            yield break;
        }

        ApplyTyped((T)grabbableObject);
    }
}