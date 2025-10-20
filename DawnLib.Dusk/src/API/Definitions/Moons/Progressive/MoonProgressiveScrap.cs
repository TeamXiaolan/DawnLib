using UnityEngine;

namespace Dusk;
public class MoonProgressiveScrap : GrabbableObject
{
    [field: SerializeReference]
    public DuskMoonReference MoonReference { get; private set; } = null!;
}