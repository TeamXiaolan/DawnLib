using System;

namespace CodeRebirthLib.ContentManagement.Unlockables;

[Serializable]
public class CRUnlockableReference(string name) : CRContentReference<CRUnlockableDefinition>(name)
{
    protected override string GetEntityName(CRUnlockableDefinition obj) => obj.UnlockableItem.unlockableName;

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
        if (obj) return new CRUnlockableReference(obj!.UnlockableItem.unlockableName);
        return null;
    }
}