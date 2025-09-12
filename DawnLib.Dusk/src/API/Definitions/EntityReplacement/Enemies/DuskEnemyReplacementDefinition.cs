using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Enemy Replacement Definition", menuName = $"{DuskModConstants.EntityReplacements}/Enemy Replacement Definition")]
public class DuskEnemyReplacementDefinition : DuskEntityReplacementDefinition
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
}
