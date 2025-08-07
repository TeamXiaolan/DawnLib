using System;

namespace CodeRebirthLib.ContentManagement.Items;

[Serializable]
public class CRItemReference(string name) : CRContentReference<CRItemDefinition>(name)
{
    protected override string GetEntityName(CRItemDefinition obj) => obj.Item.itemName;

    public static implicit operator CRItemDefinition?(CRItemReference reference)
    {
        if (CRMod.AllItems().TryGetFromItemName(reference.entityName, out var obj))
        {
            return obj;
        }
        return null;
    }

    public static implicit operator CRItemReference?(CRItemDefinition? obj)
    {
        if (obj) return new CRItemReference(obj!.Item.itemName);
        return null;
    }
}