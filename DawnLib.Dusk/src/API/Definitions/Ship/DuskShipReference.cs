using Dawn;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dusk;

internal class DuskShipReference : DuskContentReference<DuskShipDefinition, DuskShipDefinition>
{
    public DuskShipReference() : base()
    { }

    public DuskShipReference(NamespacedKey<DuskShipDefinition> key) : base(key)
    { }

    public override bool TryResolve(out DuskShipDefinition info)
    {
        return DuskModContent.Ships.TryGetValue(TypedKey, out info);
    }

    public override DuskShipDefinition Resolve()
    {
        return DuskModContent.Ships[TypedKey];
    }
}
