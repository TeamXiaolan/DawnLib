using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Enemies;

[Serializable]
public class CREnemyDefinitionReference
{
    [SerializeField]
    private string enemyAsset;

    [SerializeField]
    private string enemyName;

    public string EnemyName => enemyName;

    public static implicit operator string?(CREnemyDefinitionReference reference)
    {
        return reference.EnemyName;
    }

    public static implicit operator CREnemyDefinition?(CREnemyDefinitionReference reference)
    {
        if (CRMod.AllEnemies().TryGetFromEnemyName(reference.enemyName, out var enemy))
        {
            return enemy;
        }
        return null;
    }
}