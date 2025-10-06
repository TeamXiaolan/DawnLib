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
                if (gameObjectAddon.GameObjectToCreate.TryGetComponent(out NetworkObject networkObject) && !NetworkManager.Singleton.IsServer)
                    continue;

                GameObject addOn = GameObject.Instantiate(gameObjectAddon.GameObjectToCreate, gameObject.transform);
                addOn.transform.position = gameObjectAddon.PositionOffset + gameObject.transform.position;
                addOn.transform.rotation = gameObjectAddon.RotationOffset * addOn.transform.rotation;

                if (networkObject == null)
                    continue;

                networkObject.AutoObjectParentSync = false;
                networkObject.Spawn();
            }
        }

        if (AnimationClipReplacements.Count > 0)
        {
            Animator? grabbableAnimator = grabbableObject.GetComponentInChildren<Animator>();
            if (grabbableAnimator != null)
            {
                AnimatorOverrideController animatorOverrideController = new(grabbableAnimator.runtimeAnimatorController);
                foreach (AnimationClipReplacement animationClipReplacement in AnimationClipReplacements)
                {
                    foreach (AnimationEventAddition animationEventAddition in animationClipReplacement.PotentialAnimationEvents)
                    {
                        AnimationEvent animationEvent = new()
                        {
                            functionName = animationEventAddition.AnimationEventName,
                            time = animationEventAddition.Time,

                            stringParameter = animationEventAddition.StringParameter,
                            intParameter = animationEventAddition.IntParameter,
                            floatParameter = animationEventAddition.FloatParameter,
                            objectReferenceParameter = animationEventAddition.ObjectParameter
                        };

                        animationClipReplacement.NewAnimationClip.AddEvent(animationEvent);
                    }
                    animatorOverrideController[animationClipReplacement.OriginalClipName] = animationClipReplacement.NewAnimationClip;
                }
                grabbableAnimator.runtimeAnimatorController = animatorOverrideController;
            }
        }

        foreach (ParticleSystemReplacement particleSystemReplacement in ExtraParticleSystemReplacements)
        {
            if (string.IsNullOrEmpty(particleSystemReplacement.PathToParticleSystem))
            {
                continue;
            }

            GameObject? oldGameObject = grabbableObject.transform.Find(particleSystemReplacement.PathToParticleSystem)?.gameObject;
            if (oldGameObject != null)
            {
                GameObject newGameObject = GameObject.Instantiate(particleSystemReplacement.NewParticleSystem.gameObject, oldGameObject.transform);
                newGameObject.name = oldGameObject.name;
                Destroy(oldGameObject);
            }
        }
    }
}