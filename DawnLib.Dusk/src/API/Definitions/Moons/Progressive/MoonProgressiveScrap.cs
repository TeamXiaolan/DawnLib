using UnityEngine;

namespace Dusk;

[AddComponentMenu($"{DuskModConstants.ProgressiveComponents}/Moon Progressive Scrap")]
public class MoonProgressiveScrap : GrabbableObject
{
    [field: SerializeReference]
    public DuskMoonReference MoonReference { get; private set; } = null!;
}