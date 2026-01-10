using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Dawn;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk;

[CreateAssetMenu(fileName = "New Surface Definition", menuName = $"{DuskModConstants.Definitions}/Surface Definition")]
public class DuskSurfaceDefinition : DuskContentDefinition<DawnSurfaceInfo>
{
    [field: SerializeField]
    public FootstepSurface Surface { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);

        DawnLib.DefineSurface(TypedKey, Surface, ApplyTagsTo);
    }

    public override void TryNetworkRegisterAssets() { }

    protected override string EntityNameReference => Surface.surfaceTag;
}