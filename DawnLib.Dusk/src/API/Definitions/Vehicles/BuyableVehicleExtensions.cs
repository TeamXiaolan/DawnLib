using System;
using System.Diagnostics.CodeAnalysis;
using Dawn;
using Dawn.Internal;
using Dawn.Interfaces;

namespace Dusk;

public static class BuyableVehicleExtensions
{
    public static NamespacedKey<DuskVehicleDefinition> ToNamespacedKey(this BuyableVehicle buyableVehicle)
    {
        if (!buyableVehicle.TryGetDuskDefinition(out DuskVehicleDefinition? buyableVehicleDefinition))
        {
            Debuggers.Unlockables?.Log($"BuyableVehicle {buyableVehicle} has no DuskDefinition");
            throw new Exception();
        }
        return buyableVehicleDefinition.TypedKey.AsTyped<DuskVehicleDefinition>();
    }

    internal static bool TryGetDuskDefinition(this BuyableVehicle buyableVehicle, [NotNullWhen(true)] out DuskVehicleDefinition? buyableVehicleDefinition)
    {
        buyableVehicleDefinition = (DuskVehicleDefinition)((IDawnObject)buyableVehicle).DawnInfo;
        return buyableVehicleDefinition != null;
    }

    internal static void SetDuskDefinition(this BuyableVehicle buyableVehicle, DuskVehicleDefinition buyableVehicleDefinition)
    {
        ((IDawnObject)buyableVehicle).DawnInfo = buyableVehicleDefinition;
    }
}
