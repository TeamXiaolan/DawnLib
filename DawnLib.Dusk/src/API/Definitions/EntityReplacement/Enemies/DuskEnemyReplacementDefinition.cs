using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Enemy Replacement Definition", menuName = $"{DuskModConstants.EntityReplacements}/Enemy Replacement Definition")]
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
    }
}
