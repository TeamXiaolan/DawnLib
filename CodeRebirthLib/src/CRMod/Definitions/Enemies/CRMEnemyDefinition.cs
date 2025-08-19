using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.CRMod;

[Flags]
public enum SpawnTable
{
    Inside = 1 << 0,
    Outside = 1 << 1,
    Daytime = 1 << 2,
}

[CreateAssetMenu(fileName = "New Enemy Definition", menuName = "CodeRebirthLib/Definitions/Enemy Definition")]
public class CRMEnemyDefinition : CRMContentDefinition<EnemyData, CREnemyInfo>
{
    public const string REGISTRY_ID = "enemies";

    [field: FormerlySerializedAs("enemyType")]
    [field: SerializeField]
    public EnemyType EnemyType { get; private set; }

    [field: SerializeField]
    public SpawnTable SpawnTable { get; private set; }

    [field: FormerlySerializedAs("terminalNode")]
    [field: SerializeField]
    public TerminalNode? TerminalNode { get; private set; }

    [field: FormerlySerializedAs("terminalKeyword")]
    [field: SerializeField]
    public TerminalKeyword? TerminalKeyword { get; private set; }

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
    public EnemyConfig Config { get; private set; }

    public override void Register(CRMod mod, EnemyData data)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateEnemyConfig(section, data, SpawnWeights, EnemyType.enemyName);

        EnemyType enemy = EnemyType;
        enemy.MaxCount = Config.MaxSpawnCount.Value;
        enemy.PowerLevel = Config.PowerLevel.Value;

        SpawnWeights.SetupSpawnWeightsPreset(Config.MoonSpawnWeights?.Value ?? data.moonSpawnWeights, Config.InteriorSpawnWeights?.Value ?? data.interiorSpawnWeights, Config.WeatherSpawnWeights?.Value ?? data.weatherSpawnWeights);

        CRLib.DefineEnemy(TypedKey, EnemyType, builder =>
        {
            if (SpawnTable.HasFlag(SpawnTable.Daytime))
            {
                builder.DefineDaytime(daytimeBuilder =>
                {
                    daytimeBuilder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(SpawnWeights));
                });
            }

            if (SpawnTable.HasFlag(SpawnTable.Outside))
            {
                builder.DefineOutside(outsideBuilder =>
                {
                    outsideBuilder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(SpawnWeights));
                });
            }

            if (SpawnTable.HasFlag(SpawnTable.Inside))
            {
                builder.DefineInside(insideBuilder =>
                {
                    insideBuilder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(SpawnWeights));
                });
            }
        });
        // TODO make the bestiaries
    }

    public static EnemyConfig CreateEnemyConfig(ConfigContext section, EnemyData data, SpawnWeightsPreset spawnWeightsPreset, string enemyName)
    {
        return new EnemyConfig
        {
            MoonSpawnWeights = data.generateSpawnWeightsConfig ? section.Bind($"{enemyName} | Preset Moon Weights", $"Preset moon weights for {enemyName}.", spawnWeightsPreset.MoonSpawnWeightsTransformer.ToConfigString()) : null,
            InteriorSpawnWeights = data.generateSpawnWeightsConfig ? section.Bind($"{enemyName} | Preset Interior Weights", $"Preset interior weights for {enemyName}.", spawnWeightsPreset.InteriorSpawnWeightsTransformer.ToConfigString()) : null,
            WeatherSpawnWeights = data.generateSpawnWeightsConfig ? section.Bind($"{enemyName} | Preset Weather Weights", $"Preset weather weights for {enemyName}.", spawnWeightsPreset.WeatherSpawnWeightsTransformer.ToConfigString()) : null,
            PowerLevel = section.Bind($"{enemyName} | Power Level", $"Power level for {enemyName}.", data.powerLevel),
            MaxSpawnCount = section.Bind($"{enemyName} | Max Spawn Count", $"Max spawn count for {enemyName}.", data.maxSpawnCount),
        };
    }

    public override List<EnemyData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.enemies).ToList();
        // probably should be cached but i dont care anymore.
    }

    protected override string EntityNameReference => EnemyType.enemyName;
}