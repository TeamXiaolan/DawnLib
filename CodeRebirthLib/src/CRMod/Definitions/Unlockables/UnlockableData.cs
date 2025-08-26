using System;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class UnlockableData : EntityData<CRMUnlockableReference>
{
    public int cost;
    public bool isShipUpgrade;
    public bool isDecor;
    public bool isProgressive;
    public bool createProgressiveConfig;
}