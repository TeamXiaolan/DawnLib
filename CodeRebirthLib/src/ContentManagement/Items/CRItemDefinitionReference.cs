using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Items;

[Serializable]
public class CRItemDefinitionReference
{
    [SerializeField]
    private string itemAsset;

    [SerializeField]
    private string itemName;

    public string ItemName => itemName;

    public static implicit operator string?(CRItemDefinitionReference reference)
    {
        return reference.ItemName;
    }

    public static implicit operator CRItemDefinition?(CRItemDefinitionReference reference)
    {
        if (CRMod.AllItems().TryGetFromItemName(reference.itemName, out var item))
        {
            return item;
        }
        return null;
    }
}