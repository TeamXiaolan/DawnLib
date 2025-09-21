using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dusk.Weights;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk;

[Flags]
public enum SpawnTable
{
    Inside = 1 << 0,
    Outside = 1 << 1,
    Daytime = 1 << 2,
}

[CreateAssetMenu(fileName = "New Enemy Definition", menuName = $"{DuskModConstants.Definitions}/Enemy Definition")]
public class DuskEnemyDefinition : DuskContentDefinition<DawnEnemyInfo>
{
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

    [field: Space(10)]
    [field: Header("Configs | Spawn Weights")]
    [field: SerializeField]
    public string MoonSpawnWeights { get; private set; }
    [field: SerializeField]
    public string InteriorSpawnWeights { get; private set; }
    [field: SerializeField]
    public string WeatherSpawnWeights { get; private set; }
    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; }

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
    public EnemyConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateEnemyConfig(section);

        EnemyType enemy = EnemyType;
        enemy.MaxCount = Config.MaxSpawnCount.Value;
        enemy.PowerLevel = Config.PowerLevel.Value;

        SpawnWeights.SetupSpawnWeightsPreset(Config.MoonSpawnWeights?.Value ?? MoonSpawnWeights, Config.InteriorSpawnWeights?.Value ?? InteriorSpawnWeights, Config.WeatherSpawnWeights?.Value ?? WeatherSpawnWeights);

        DawnLib.DefineEnemy(TypedKey, EnemyType, builder =>
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

            if (TerminalKeyword != null)
                builder.OverrideNameKeyword(TerminalKeyword);

            if (TerminalNode != null)
                builder.SetBestiaryNode(TerminalNode);

            ApplyTagsTo(builder);
        });
    }

    public EnemyConfig CreateEnemyConfig(ConfigContext section)
    {
        return new EnemyConfig
        {
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", SpawnWeights.MoonSpawnWeightsTransformer.ToConfigString()) : null,
            InteriorSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Interior Weights", $"Preset interior weights for {EntityNameReference}.", SpawnWeights.InteriorSpawnWeightsTransformer.ToConfigString()) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", SpawnWeights.WeatherSpawnWeightsTransformer.ToConfigString()) : null,

            PowerLevel = section.Bind($"{EntityNameReference} | Power Level", $"Power level for {EntityNameReference}.", EnemyType.PowerLevel),
            MaxSpawnCount = section.Bind($"{EntityNameReference} | Max Spawn Count", $"Max spawn count for {EntityNameReference}.", EnemyType.MaxCount),
        };
    }

    protected override string EntityNameReference => EnemyType?.enemyName ?? string.Empty;
}