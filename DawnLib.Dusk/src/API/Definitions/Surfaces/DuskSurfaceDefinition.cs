using Dawn;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Surface Definition", menuName = $"{DuskModConstants.Definitions}/Surface Definition")]
public class DuskSurfaceDefinition : DuskContentDefinition<DawnSurfaceInfo>
{
    [field: SerializeField]
    public FootstepSurface Surface { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Please make sure this is a prefab with a ParticleSystem!")]
    public GameObject? FootstepVFXPrefab { get; private set; }

    [field: SerializeField]
    public Vector3 PositionOffset { get; private set; } = Vector3.zero;

    public override void Register(DuskMod mod)
    {
        base.Register(mod);

        if (FootstepVFXPrefab != null && FootstepVFXPrefab.GetComponentInChildren<ParticleSystem>() == null)
        {
            DuskPlugin.Logger.LogError($"The FootstepVFXPrefab: {FootstepVFXPrefab.name} has no ParticleSystem component, removed.");
            FootstepVFXPrefab = null;
        }

        DawnLib.DefineSurface(TypedKey, Surface, builder =>
        {
            builder.SetSurfaceVFXPrefab(FootstepVFXPrefab);
            builder.OverrideSurfaceVFXOffset(PositionOffset);
            ApplyTagsTo(builder);
        });
    }

    public override void TryNetworkRegisterAssets() { }

    protected override string EntityNameReference => Surface.surfaceTag;
}