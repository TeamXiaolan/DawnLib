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

    public override void Apply(GrabbableObject grabbableObject)
    {
    }
}

public abstract class DuskItemReplacementDefinition<T> : DuskItemReplacementDefinition where T : GrabbableObject
{
    protected abstract void Apply(T grabbableObject);
    public override void Apply(GrabbableObject grabbableObject)
    {
        base.Apply(grabbableObject);
        Apply((T)grabbableObject);

        foreach (SkinnedMeshReplacement skinnedMeshReplacement in SkinnedMeshReplacements)
        {
            if (string.IsNullOrEmpty(skinnedMeshReplacement.PathToRenderer))
            {
                continue;
            }

            GameObject? gameObject = grabbableObject.transform.Find(skinnedMeshReplacement.PathToRenderer)?.gameObject;
            if (gameObject != null)
            {
                TransferRenderer transferRenderer = gameObject.AddComponent<TransferRenderer>();
                transferRenderer.RendererReplacement = skinnedMeshReplacement;
            }
        }

        foreach (MeshReplacement meshReplacement in MeshReplacements)
        {
            if (string.IsNullOrEmpty(meshReplacement.PathToRenderer))
            {
                continue;
            }

            GameObject? gameObject = grabbableObject.transform.Find(meshReplacement.PathToRenderer)?.gameObject;
            if (gameObject != null)
            {
                TransferRenderer transferRenderer = gameObject.AddComponent<TransferRenderer>();
                transferRenderer.RendererReplacement = meshReplacement;
            }
        }
    }
}