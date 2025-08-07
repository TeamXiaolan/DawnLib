using System;

namespace CodeRebirthLib.ContentManagement.Unlockables;
[Serializable]
public class UnlockableData : EntityData<CRUnlockableReference>
{
    public int cost;
    public bool isShipUpgrade;
    public bool isDecor;
    public bool isProgressive;
    public bool createProgressiveConfig;
}