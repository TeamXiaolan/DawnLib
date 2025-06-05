using System;

namespace CodeRebirthLib.ContentManagement.Enemies;
[Serializable]
public class EnemyData : EntityData
{
    public string spawnWeights;
    public string weatherMultipliers;
    public float powerLevel;
    public int maxSpawnCount;
}
