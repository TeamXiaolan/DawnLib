using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.MapObjects;

[Serializable]
public class CRMapObjectReference(string name) : CRContentReference<CRMapObjectDefinition>(name)
{
    protected override string GetEntityName(CRMapObjectDefinition obj) => obj.MapObjectName;

    public static implicit operator CRMapObjectDefinition?(CRMapObjectReference reference)
    {
        if (CRMod.AllMapObjects().TryGetFromMapObjectName(reference.entityName, out var obj))
        {
            return obj;
        }
        return null;
    }

    public static implicit operator CRMapObjectReference?(CRMapObjectDefinition? obj)
    {
        if (obj) return new CRMapObjectReference(obj!.MapObjectName);
        return null;
    }
}