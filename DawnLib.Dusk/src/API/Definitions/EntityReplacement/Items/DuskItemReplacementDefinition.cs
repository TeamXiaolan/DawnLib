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

    public override IEnumerator Apply(GrabbableObject ai)
    {
        yield break;
    }
}

public abstract class DuskItemReplacementDefinition<T> : DuskItemReplacementDefinition where T : GrabbableObject
{
    protected abstract void ApplyTyped(T grabbableObject);
    public override IEnumerator Apply(GrabbableObject grabbableObject)
    {
        yield return base.Apply(grabbableObject);
        grabbableObject.SetGrabbableObjectReplacement(this);
        foreach (Hierarchy hierarchyReplacement in Replacements)
        {
            yield return StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(grabbableObject.transform));
        }

        foreach (GameObjectWithPath gameObjectAddon in GameObjectAddons)
        {
            if (string.IsNullOrEmpty(gameObjectAddon.PathToGameObject))
            {
                continue;
            }

            GameObject? gameObject = grabbableObject.transform.Find(gameObjectAddon.PathToGameObject)?.gameObject;
            if (gameObject != null)
            {
                if (gameObjectAddon.GameObjectToCreate.TryGetComponent(out NetworkObject networkObject) && !NetworkManager.Singleton.IsServer)
                    continue;

                GameObject addOn = GameObject.Instantiate(gameObjectAddon.GameObjectToCreate, gameObject.transform);
                addOn.transform.localPosition = gameObjectAddon.PositionOffset;
                addOn.transform.rotation = Quaternion.Euler(gameObjectAddon.RotationOffset);

                if (networkObject == null)
                    continue;

                networkObject.AutoObjectParentSync = false;
                networkObject.Spawn();
            }
        }
        ApplyTyped((T)grabbableObject);
    }
}