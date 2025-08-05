using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Unlockables;

[Serializable]
public class CRUnlockableDefinitionReference
{
    [SerializeField]
    private string unlockableAsset;

    [SerializeField]
    private string unlockableName;

    public string UnlockableName => unlockableName;

    public static implicit operator string?(CRUnlockableDefinitionReference reference)
    {
        return reference.UnlockableName;
    }

    public static implicit operator CRUnlockableDefinition?(CRUnlockableDefinitionReference reference)
    {
        if (CRMod.AllUnlockables().TryGetFromUnlockableName(reference.unlockableName, out var unlockable))
        {
            return unlockable;
        }
        return null;
    }
}