using UnityEngine;

namespace Dusk;

public abstract class DuskEnemyReplacementDefinition : DuskEntityReplacementDefinition<EnemyAI>
{
    [field: SerializeField]
    public AudioClip? HitBodySFX { get; private set; }

    [field: SerializeField]
    public AudioClip HitEnemyVoiceSFX { get; private set; }

    [field: SerializeField]
    public AudioClip DeathSFX { get; private set; }

    [field: SerializeField]
    public AudioClip StunSFX { get; private set; }

    [field: SerializeField]
    public AudioClip[] AnimVoiceClips { get; private set; } = [];

    [field: SerializeField]
    public AudioClip[] AudioClips { get; private set; } = [];

    public override void Apply(EnemyAI enemyAI)
    {
    }
}

public abstract class DuskEnemyReplacementDefinition<T> : DuskEnemyReplacementDefinition where T : EnemyAI
{
    protected abstract void Apply(T enemyAI);
    public override void Apply(EnemyAI enemyAI)
    {
        base.Apply(enemyAI);

        Apply((T)enemyAI);

        foreach (SkinnedMeshReplacement skinnedMeshReplacement in SkinnedMeshReplacements)
        {
            if (string.IsNullOrEmpty(skinnedMeshReplacement.PathToRenderer))
            {
                continue;
            }

            GameObject? gameObject = enemyAI.transform.Find(skinnedMeshReplacement.PathToRenderer)?.gameObject;
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

            GameObject? gameObject = enemyAI.transform.Find(meshReplacement.PathToRenderer)?.gameObject;
            if (gameObject != null)
            {
                TransferRenderer transferRenderer = gameObject.AddComponent<TransferRenderer>();
                transferRenderer.RendererReplacement = meshReplacement;
            }
        }

        foreach (GameObjectWithPath gameObjectAddon in GameObjectAddons)
        {
            GameObject? gameObject = enemyAI.transform.Find(gameObjectAddon.PathToGameObject)?.gameObject;
            if (gameObject != null)
            {
                GameObject addOn = GameObject.Instantiate(gameObjectAddon.GameObjectToCreate, gameObject.transform);
                addOn.transform.position = gameObject.transform.position;
                addOn.transform.rotation = gameObjectAddon.Rotation * addOn.transform.rotation;
            }
        }
    }
}