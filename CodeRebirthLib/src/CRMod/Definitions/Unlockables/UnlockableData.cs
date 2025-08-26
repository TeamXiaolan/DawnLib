using System;
using UnityEngine;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class UnlockableData : EntityData<CRMUnlockableReference>, IInspectorHeaderWarning
{
    public bool TryGetHeaderWarning(out string? message)
    {
        message = null;
        return false;
    }

    public int cost;
    public bool isShipUpgrade;
    public bool isDecor;
    public bool isProgressive;
    public bool createProgressiveConfig;
}