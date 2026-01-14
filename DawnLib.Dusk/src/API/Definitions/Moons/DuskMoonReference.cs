using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskMoonReference : DuskContentReference<DuskMoonDefinition, DawnMoonInfo>
{
    public DuskMoonReference() : base() { }
    public DuskMoonReference(NamespacedKey<DawnMoonInfo> key) : base(key) { }

    public override bool TryResolve(out DawnMoonInfo info)
    {
        return LethalContent.Moons.TryGetValue(TypedKey, out info);
    }
    public override DawnMoonInfo Resolve()
    {
        return LethalContent.Moons[TypedKey];
    }
}