using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

public abstract class DuskEnemyReplacementDefinition : DuskEntityReplacementDefinition<EnemyAI>
{

    [field: Header("Nest")]
    [field: SerializeField]
    public List<RendererReplacement> NestRendererReplacements { get; private set; }

    [field: Header("EnemyType Audio")]
    [field: SerializeField]
    public AudioClip? HitBodySFX { get; private set; }

    [field: SerializeField]
    public AudioClip? HitEnemyVoiceSFX { get; private set; }

    [field: SerializeField]
    public AudioClip? StunSFX { get; private set; }

    [field: SerializeField]
    public AudioClip[] AudioClips { get; private set; } = [];

    public virtual void ApplyNest(EnemyAINestSpawnObject nest)
    {
        EnemyType type = nest.enemyType;
        if (type.useMinEnemyThresholdForNest)
        {
            return;
        }

        foreach (RendererReplacement rendererReplacement in NestRendererReplacements)
        {
            if (string.IsNullOrEmpty(rendererReplacement.PathToRenderer))
            {
                continue;
            }

            GameObject? gameObject = nest.transform.Find(rendererReplacement.PathToRenderer)?.gameObject;
            if (gameObject != null)
            {
                TransferRenderer transferRenderer = gameObject.AddComponent<TransferRenderer>();
                transferRenderer.RendererReplacement = rendererReplacement;
            }
        }

        nest.SetNestReplacement(this);
    }
}

public abstract class DuskEnemyReplacementDefinition<T> : DuskEnemyReplacementDefinition where T : EnemyAI
{
    protected abstract void Apply(T enemyAI);
    public override void Apply(EnemyAI enemyAI)
    {
        Apply((T)enemyAI);
        enemyAI.SetEnemyReplacement(this);
        foreach (RendererReplacement rendererReplacement in RendererReplacements)
        {
            if (string.IsNullOrEmpty(rendererReplacement.PathToRenderer))
            {
                continue;
            }

            GameObject? gameObject = enemyAI.transform.Find(rendererReplacement.PathToRenderer)?.gameObject;
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

            GameObject? gameObject = enemyAI.transform.Find(gameObjectAddon.PathToGameObject)?.gameObject;
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

        if (AnimationClipReplacements.Count > 0 && enemyAI.creatureAnimator != null)
        {
            AnimatorOverrideController animatorOverrideController = new(enemyAI.creatureAnimator.runtimeAnimatorController);
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
            enemyAI.creatureAnimator.runtimeAnimatorController = animatorOverrideController;
        }

        foreach (ParticleSystemReplacement particleSystemReplacement in ExtraParticleSystemReplacements)
        {
            if (string.IsNullOrEmpty(particleSystemReplacement.PathToParticleSystem))
            {
                continue;
            }

            GameObject? oldGameObject = enemyAI.transform.Find(particleSystemReplacement.PathToParticleSystem)?.gameObject;
            if (oldGameObject != null)
            {
                GameObject newGameObject = GameObject.Instantiate(particleSystemReplacement.NewParticleSystem.gameObject, oldGameObject.transform);
                newGameObject.name = oldGameObject.name;
                Destroy(oldGameObject);
            }
        }
    }
}