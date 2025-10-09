using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

public class DuskEnemyReplacementDefinition : DuskEntityReplacementDefinition<EnemyAI>
{

    [field: Header("Nest")]
    [field: SerializeField]
    public List<HierarchyReplacement> NestRendererReplacements { get; private set; } = new();

    [field: Header("EnemyType Audio")]
    [field: SerializeField]
    public AudioClip? HitBodySFX { get; private set; }

    [field: SerializeField]
    public AudioClip? HitEnemyVoiceSFX { get; private set; }

    [field: SerializeField]
    public AudioClip? StunSFX { get; private set; }

    [field: SerializeField]
    public AudioClip[] AudioClips { get; private set; } = [];

    public override void Apply(EnemyAI ai) { }

    public virtual void ApplyNest(EnemyAINestSpawnObject nest)
    {
        EnemyType type = nest.enemyType;
        if (type.useMinEnemyThresholdForNest)
        {
            return;
        }

        foreach (HierarchyReplacement hierarchyReplacement in NestRendererReplacements)
        {
            hierarchyReplacement.Apply(nest.transform);
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
        foreach (HierarchyReplacement hierarchyReplacement in Replacements)
        {
            hierarchyReplacement.Apply(enemyAI.transform);
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
                addOn.transform.rotation = Quaternion.Euler(gameObjectAddon.RotationOffset) * addOn.transform.rotation;

                if (networkObject == null)
                    continue;

                networkObject.AutoObjectParentSync = false;
                networkObject.Spawn();
            }
        }
    }
}