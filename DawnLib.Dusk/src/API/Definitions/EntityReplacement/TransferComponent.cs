using UnityEngine;
using UnityEngine.VFX;

namespace Dusk;

public class TransferComponent : MonoBehaviour
{
    public Component ComponentReplacement { get; internal set; }

    private void Start()
    {
        if (ComponentReplacement == null)
        {
            DuskPlugin.Logger.LogError("TransferComponent: ComponentReplacement is null.");
            return;
        }

        if (ComponentReplacement is ParticleSystem particleSystem)
        {
            if (TryGetComponent(out ParticleSystem targetParticleSystem))
            {
                ReplaceParticleSystem(particleSystem, targetParticleSystem);
                Destroy(this);
                return;
            }
            else
            {
                DuskPlugin.Logger.LogError("TransferComponent: Target has no ParticleSystem but replacement is ParticleSystem.");
                return;
            }
        }

        if (ComponentReplacement is VisualEffect visualEffect)
        {
            if (TryGetComponent(out VisualEffect targetVisualEffect))
            {
                ReplaceVisualEffect(visualEffect, targetVisualEffect);
                Destroy(this);
                return;
            }
            else
            {
                DuskPlugin.Logger.LogError("TransferComponent: Target has no VisualEffect but replacement is VisualEffect.");
                return;
            }
        }

        DuskPlugin.Logger.LogError($"TransferComponent: Unsupported ComponentReplacement type: {ComponentReplacement.GetType().Name}");
    }

    private static void ReplaceParticleSystem(ParticleSystem particleSystem, ParticleSystem targetParticleSystem)
    {
        // todo
    }

    private static void ReplaceVisualEffect(VisualEffect visualEffect, VisualEffect targetVisualEffect)
    {
        targetVisualEffect.visualEffectAsset = visualEffect.visualEffectAsset;
        // todo
    }
}