using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

public abstract class DuskEnemyReplacementDefinition : DuskEntityReplacementDefinition<EnemyAI>
{
    [field: SerializeField]
    public List<RendererReplacement> NestedRendererReplacements { get; private set; }

    [field: Space(10f)]
    [field: SerializeField]
    public AudioClip? HitBodySFX { get; private set; }

    [field: SerializeField]
    public AudioClip? HitEnemyVoiceSFX { get; private set; }

    [field: SerializeField]
    public AudioClip? StunSFX { get; private set; }

    [field: SerializeField]
    public AudioClip[] AnimVoiceClips { get; private set; } = [];

    [field: SerializeField]
    public AudioClip[] AudioClips { get; private set; } = [];

    public virtual void ApplyNest(EnemyAINestSpawnObject nest)
    {
        EnemyType type = nest.enemyType;
        if (type.useMinEnemyThresholdForNest)
        {
            return;
        }

        foreach (RendererReplacement rendererReplacement in NestedRendererReplacements)
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
    }
}