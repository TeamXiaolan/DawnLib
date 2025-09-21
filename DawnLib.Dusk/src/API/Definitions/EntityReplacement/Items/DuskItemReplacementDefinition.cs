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
        base.Apply(grabbableObject);

        Apply((T)grabbableObject);

        foreach (RendererReplacement rendererReplacement in RendererReplacements)
        {
            if (string.IsNullOrEmpty(rendererReplacement.PathToRenderer))
            {
                continue;
            }

            GameObject? gameObject = grabbableObject.transform.Find(rendererReplacement.PathToRenderer)?.gameObject;
            if (gameObject != null)
            {
                TransferRenderer transferRenderer = gameObject.AddComponent<TransferRenderer>();
                transferRenderer.RendererReplacement = rendererReplacement;
            }
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
                GameObject addOn = GameObject.Instantiate(gameObjectAddon.GameObjectToCreate, gameObject.transform);
                addOn.transform.position = gameObject.transform.position;
                addOn.transform.rotation = gameObjectAddon.Rotation * addOn.transform.rotation;
            }
        }
    }
}