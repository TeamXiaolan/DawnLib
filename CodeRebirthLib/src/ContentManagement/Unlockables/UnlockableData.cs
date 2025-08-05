using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Unlockables;
[Serializable]
public class UnlockableData : EntityData
{
    [SerializeReference]
    public CRUnlockableReference unlockableReference = new(string.Empty);
    public override string EntityName => unlockableReference.entityName;

    public int cost;
    public bool isShipUpgrade;
    public bool isDecor;
    public bool isProgressive;
    public bool createProgressiveConfig;
}