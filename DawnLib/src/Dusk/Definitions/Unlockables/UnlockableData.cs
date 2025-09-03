using System;
using UnityEngine.Serialization;

namespace Dawn.Dusk;
[Serializable]
public class UnlockableData : EntityData<CRMUnlockableReference>
{
    public int cost;
    public bool isShipUpgrade;
    public bool isDecor;
    [FormerlySerializedAs("createProgressiveConfig")] public bool generateDisableUnlockRequirementConfig;
}