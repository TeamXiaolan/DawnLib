using System;
using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ConfigManagement.Weights;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.Enemies;

[Flags]
public enum SpawnTable
{
    Inside = 1 << 0,
    Outside = 1 << 1,
    Daytime = 1 << 2,
}

[CreateAssetMenu(fileName = "New Enemy Definition", menuName = "CodeRebirthLib/Definitions/Enemy Definition")]
public class CREnemyDefinition : CRContentDefinition<EnemyData>
{
    public const string REGISTRY_ID = "enemies";

    [field: FormerlySerializedAs("enemyType")]
    [field: SerializeField]
    public EnemyType EnemyType { get; private set; }

    [field: SerializeField]
    public SpawnTable SpawnTable { get; private set; }

    [field: SerializeField]
    public SpawnWeightsPreset SpawnWeights { get; private set; }

    [field: FormerlySerializedAs("terminalNode")]
    [field: SerializeField]
    public TerminalNode? TerminalNode { get; private set; }

    [field: FormerlySerializedAs("terminalKeyword")]
    [field: SerializeField]
    public TerminalKeyword? TerminalKeyword { get; private set; }

    public EnemyConfig Config { get; private set; }

    protected override string EntityNameReference => EnemyType.enemyName;

    public override void Register(CRMod mod, EnemyData data)
    {
        if (SpawnWeights == null)
        {
            SpawnWeights = ScriptableObject.CreateInstance<SpawnWeightsPreset>();
        }

        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateEnemyConfig(section, data, SpawnWeights, EnemyType.enemyName);

        EnemyType enemy = EnemyType;
        enemy.MaxCount = Config.MaxSpawnCount.Value;
        enemy.PowerLevel = Config.PowerLevel.Value;

        if (Config.MoonSpawnWeights != null && Config.InteriorSpawnWeights != null && Config.WeatherSpawnWeights != null)
        {
            SpawnWeights.SetupSpawnWeightsPreset(Config.MoonSpawnWeights.Value, Config.InteriorSpawnWeights.Value, Config.WeatherSpawnWeights.Value);
        }

        CRLib.RegisterEnemy(EnemyType, "All", SpawnWeights);
        // TODO make the bestiaries
        mod.EnemyRegistry().Register(this);
    }

    internal static void UpdateAllWeights(SelectableLevel? level = null)
    {
        if (!StartOfRound.Instance)
            return;

        SelectableLevel levelToUpdate = level ?? StartOfRound.Instance.currentLevel;

        foreach (var spawnableEnemyWithRarity in levelToUpdate.Enemies)
        {
            if (!spawnableEnemyWithRarity.enemyType.TryGetDefinition(out CREnemyDefinition? definition))
                continue;

            spawnableEnemyWithRarity.rarity = definition.SpawnWeights.GetWeight();
        }

        foreach (var spawnableEnemyWithRarity in levelToUpdate.OutsideEnemies)
        {
            if (!spawnableEnemyWithRarity.enemyType.TryGetDefinition(out CREnemyDefinition? definition))
                continue;

            spawnableEnemyWithRarity.rarity = definition.SpawnWeights.GetWeight();
        }

        foreach (var spawnableEnemyWithRarity in levelToUpdate.DaytimeEnemies)
        {
            if (!spawnableEnemyWithRarity.enemyType.TryGetDefinition(out CREnemyDefinition? definition))
                continue;

            spawnableEnemyWithRarity.rarity = definition.SpawnWeights.GetWeight();
        }
    }

    public static EnemyConfig CreateEnemyConfig(ConfigContext section, EnemyData data, SpawnWeightsPreset spawnWeightsPreset, string enemyName)
    {
        return new EnemyConfig
        {
            // todo, take the old weather and moon spawn weights and parse em into
            MoonSpawnWeights = data.generateSpawnWeightsConfig ? section.Bind($"{enemyName} | Preset Moon Weights", $"Preset moon weights for {enemyName}.", spawnWeightsPreset.MoonSpawnWeightsTransformer.ToConfigString()) : null,
            InteriorSpawnWeights = data.generateSpawnWeightsConfig ? section.Bind($"{enemyName} | Preset Interior Weights", $"Preset interior weights for {enemyName}.", spawnWeightsPreset.InteriorSpawnWeightsTransformer.ToConfigString()) : null,
            WeatherSpawnWeights = data.generateSpawnWeightsConfig ? section.Bind($"{enemyName} | Preset Weather Weights", $"Preset weather weights for {enemyName}.", spawnWeightsPreset.WeatherSpawnWeightsTransformer.ToConfigString()) : null,
            PowerLevel = section.Bind($"{enemyName} | Power Level", $"Power level for {enemyName}.", data.powerLevel),
            MaxSpawnCount = section.Bind($"{enemyName} | Max Spawn Count", $"Max spawn count for {enemyName}.", data.maxSpawnCount),
        };
    }

    public static void RegisterTo(CRMod mod)
    {
        mod.CreateRegistry(REGISTRY_ID, new CRRegistry<CREnemyDefinition>());
    }

    public override List<EnemyData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.enemies).ToList();
        // probably should be cached but i dont care anymore.
    }
}