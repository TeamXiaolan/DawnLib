using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Enemies;
[Serializable]
public class EnemyData : EntityData
{
    [SerializeReference]
    public CREnemyReference enemyReference = new(string.Empty);
    public override string EntityName => enemyReference.entityName;

    public string spawnWeights;
    public string weatherMultipliers;
    public float powerLevel;
    public int maxSpawnCount;
}