using System;

namespace CodeRebirthLib;
[Serializable]
public class UnlockableData : EntityData<CRUnlockableReference>
{
    public int cost;
    public bool isShipUpgrade;
    public bool isDecor;
    public bool isProgressive;
    public bool createProgressiveConfig;
}