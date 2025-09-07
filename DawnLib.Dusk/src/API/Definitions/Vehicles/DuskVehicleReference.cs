using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskVehicleReference : DuskContentReference<DuskVehicleDefinition, DuskVehicleDefinition>
{
    public DuskVehicleReference() : base()
    { }
    public DuskVehicleReference(NamespacedKey<DuskVehicleDefinition> key) : base(key)
    { }
    public override bool TryResolve(out DuskVehicleDefinition info)
    {
        return DuskModContent.Vehicles.TryGetValue(TypedKey, out info);
    }

    public override DuskVehicleDefinition Resolve()
    {
        return DuskModContent.Vehicles[TypedKey];
    }
}