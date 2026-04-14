using System.Collections;
using System.Collections.Generic;
using Dawn.Internal;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

public class DuskEnemyReplacementDefinition : DuskEntityReplacementDefinition<EnemyAI>
{

    [field: Header("Nest")]
    [field: SerializeField]
    public List<Hierarchy> NestRendererReplacements { get; private set; } = new();

    [field: Header("EnemyType Audio")]
    [field: SerializeField]
    public AudioClip? HitBodySFX { get; private set; }

    [field: SerializeField]
    public AudioClip? HitEnemyVoiceSFX { get; private set; }

    [field: SerializeField]
    public AudioClip? StunSFX { get; private set; }

    [field: SerializeField]
    public AudioClip[] AudioClips { get; private set; } = [];

    public override IEnumerator Apply(EnemyAI ai, bool immediate = false)
    {
        yield break;
    }

    public virtual IEnumerator ApplyNest(EnemyAINestSpawnObject nest, bool immediate = false)
    {
        EnemyType type = nest.enemyType;
        if (type.useMinEnemyThresholdForNest)
        {
            yield break;
        }

        foreach (Hierarchy hierarchyReplacement in NestRendererReplacements)
        {
            if (immediate)
            {
                StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(nest.transform, immediate));
            }
            else
            {
                yield return StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(nest.transform, immediate));
            }
        }

        nest.SetNestReplacement(this);
    }
}

public abstract class DuskEnemyReplacementDefinition<T> : DuskEnemyReplacementDefinition where T : EnemyAI
{
    protected abstract void ApplyTyped(T enemyAI);
    public override IEnumerator Apply(EnemyAI enemyAI, bool immediate = false)
    {
        yield return base.Apply(enemyAI);
        enemyAI.SetEnemyReplacement(this);
        foreach (Hierarchy hierarchyReplacement in Replacements)
        {
            if (immediate)
            {
                StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(enemyAI.transform, immediate));
            }
            else
            {
                yield return StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(enemyAI.transform, immediate));
            }
        }

        foreach (GameObjectWithPath gameObjectAddon in GameObjectAddons)
        {
            GameObject gameObject = !string.IsNullOrWhiteSpace(gameObjectAddon.PathToGameObject) ? enemyAI.transform.Find(gameObjectAddon.PathToGameObject).gameObject : enemyAI.gameObject;
            if (gameObjectAddon.GameObjectToCreate.TryGetComponent(out NetworkObject networkObject) && !NetworkManager.Singleton.IsServer)
                continue;

            GameObject addOn = GameObject.Instantiate(gameObjectAddon.GameObjectToCreate, gameObject.transform);
            addOn.transform.localPosition = gameObjectAddon.PositionOffset;
            addOn.transform.localRotation = Quaternion.Euler(gameObjectAddon.RotationOffset);

            if (networkObject == null)
                continue;

            networkObject.AutoObjectParentSync = false;
            networkObject.Spawn();
        }
        ApplyTyped((T)enemyAI);
    }
}