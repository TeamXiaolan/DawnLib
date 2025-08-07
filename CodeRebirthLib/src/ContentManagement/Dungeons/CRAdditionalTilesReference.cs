using System;
using CodeRebirthLib.ContentManagement.Enemies;

namespace CodeRebirthLib.ContentManagement.Dungeons;

[Serializable]
public class CRAdditionalTilesReference(string name) : CRContentReference<CRAdditionalTilesDefinition>(name)
{
    protected override string GetEntityName(CRAdditionalTilesDefinition obj) => obj.ArchetypeName;

    public static implicit operator CRAdditionalTilesDefinition?(CRAdditionalTilesReference reference)
    {
        if (CRLibContent.AllAdditionalTiles().TryGetFromAdditionalTilesName(reference.entityName, out var obj))
        {
            return obj;
        }
        return null;
    }

    public static implicit operator CRAdditionalTilesReference?(CRAdditionalTilesDefinition? obj)
    {
        if (obj) return new CRAdditionalTilesReference(obj!.ArchetypeName);
        return null;
    }
}