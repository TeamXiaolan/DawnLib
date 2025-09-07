using System;

namespace Dusk;
[Serializable]
public class VehicleData : EntityData<DuskVehicleReference>
{
    public bool generateDisableUnlockConfig;
    public bool generateDisablePricingStrategyConfig;
    public int cost;
}