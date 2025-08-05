using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Unlockables;

[Serializable]
public class CRUnlockableReference(string name) : CRContentReference<CRUnlockableDefinition>(name)
{
    protected override string GetEntityName(CRUnlockableDefinition obj) => obj.UnlockableItemDef.unlockable.unlockableName;

    public static implicit operator CRUnlockableDefinition?(CRUnlockableReference reference)
    {
        if (CRMod.AllUnlockables().TryGetFromUnlockableName(reference.entityName, out var obj))
        {
            return obj;
        }
        return null;
    }
    
    public static implicit operator CRUnlockableReference?(CRUnlockableDefinition? obj)
    {
        if (obj) return new CRUnlockableReference(obj!.UnlockableItemDef.unlockable.unlockableName);
        return null;
    }
}