using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Enemies;
[Serializable]
public class EnemyData : EntityData<CREnemyReference>
{
    public string spawnWeights;
    public string weatherMultipliers;
    public float powerLevel;
    public int maxSpawnCount;
}