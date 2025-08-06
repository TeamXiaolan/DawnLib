using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Enemies;

[Serializable]
public class CREnemyReference(string name) : CRContentReference<CREnemyDefinition>(name)
{
    protected override string GetEntityName(CREnemyDefinition obj) => obj.EnemyType.enemyName;
    
    public static implicit operator CREnemyDefinition?(CREnemyReference reference)
    {
        if (CRLibContent.AllEnemies().TryGetFromEnemyName(reference.entityName, out var obj))
        {
            return obj;
        }
        return null;
    }
    
    public static implicit operator CREnemyReference?(CREnemyDefinition? obj)
    {
        if (obj) return new CREnemyReference(obj!.EnemyType.enemyName);
        return null;
    }
}