using System;
using UnityEngine.Serialization;

namespace Dusk;

[Serializable]
public class UnlockableData : EntityData<DuskUnlockableReference>
{
    public int cost;
    public bool isShipUpgrade;
    public bool isDecor;
    [FormerlySerializedAs("createProgressiveConfig")] public bool generateDisableUnlockRequirementConfig;
    public bool generateDisablePricingStrategyConfig;
}