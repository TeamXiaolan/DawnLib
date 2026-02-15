using Dawn;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dusk;

[Serializable]
public class DuskShipReference : DuskContentReference<DuskShipDefinition, DawnShipInfo>
{
    public DuskShipReference() : base()
    { }

    public DuskShipReference(NamespacedKey<DawnShipInfo> key) : base(key)
    { }

    public override bool TryResolve(out DawnShipInfo info)
    {
        return LethalContent.Ships.TryGetValue(TypedKey, out info);
    }

    public override DawnShipInfo Resolve()
    {
        return LethalContent.Ships[TypedKey];
    }
}