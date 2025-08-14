using System;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class EnemyData : EntityData<CRMEnemyReference>
{
    [Obsolete]
    public string spawnWeights; // to be gotten rid of
    [Obsolete]
    public string weatherMultipliers; // to be gotten rid of
    public string moonSpawnWeights;
    public string interiorSpawnWeights;
    public string weatherSpawnWeights;
    public bool generateSpawnWeightsConfig;
    public float powerLevel;
    public int maxSpawnCount;
}