using Unity.Netcode;
using UnityEngine;

namespace Dusk;

public abstract class DuskItemReplacementDefinition : DuskEntityReplacementDefinition<GrabbableObject>
{
    [field: SerializeField]
    public AudioClip? GrabSFX { get; private set; }

    [field: SerializeField]
    public AudioClip? DropSFX { get; private set; }

    [field: SerializeField]
    public AudioClip? PocketSFX { get; private set; }

    [field: SerializeField]
    public AudioClip? ThrowSFX { get; private set; }
}

public abstract class DuskItemReplacementDefinition<T> : DuskItemReplacementDefinition where T : GrabbableObject
{
    protected abstract void Apply(T grabbableObject);
    public override void Apply(GrabbableObject grabbableObject)
    {
        Apply((T)grabbableObject);
        grabbableObject.SetGrabbableObjectReplacement(this);
        foreach (HierarchyReplacement hierarchyReplacement in Replacements)
        {
            hierarchyReplacement.Apply(grabbableObject.transform);
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
                addOn.transform.position = gameObjectAddon.PositionOffset + gameObject.transform.position;
                addOn.transform.rotation = Quaternion.Euler(gameObjectAddon.RotationOffset) * addOn.transform.rotation;

                if (networkObject == null)
                    continue;

                networkObject.AutoObjectParentSync = false;
                networkObject.Spawn();
            }
        }
    }
}