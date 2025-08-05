using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.MapObjects;

[Serializable]
public class CRMapObjectDefinitionReference
{
    [SerializeField]
    private string mapObjectAsset;

    [SerializeField]
    private string mapObjectName;

    public string ItemName => mapObjectName;

    public static implicit operator string?(CRMapObjectDefinitionReference reference)
    {
        return reference.ItemName;
    }

    public static implicit operator CRMapObjectDefinition?(CRMapObjectDefinitionReference reference)
    {
        if (CRMod.AllMapObjects().TryGetFromMapObjectName(reference.mapObjectName, out var mapObject))
        {
            return mapObject;
        }
        return null;
    }
}