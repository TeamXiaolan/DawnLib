using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Util.Attributes;
using LethalLib.Modules;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.Enemies;

[CreateAssetMenu(fileName = "New Enemy Definition", menuName = "CodeRebirthLib/Definitions/Enemy Definition")]
public class CREnemyDefinition : CRContentDefinition<EnemyData>
{
    [field: FormerlySerializedAs("enemyType"), SerializeField]
    public EnemyType EnemyType { get; private set; }
    
    [field: FormerlySerializedAs("terminalNode"), SerializeField]
    public TerminalNode? TerminalNode { get; private set; }
    
    [field: FormerlySerializedAs("terminalKeyword"), SerializeField]
    public TerminalKeyword? TerminalKeyword { get; private set; }

    [HideInInspector]
    public Dictionary<string, float> WeatherMultipliers = new();
    
    public EnemyConfig Config { get; private set; }

    private Dictionary<SelectableLevel, AttributeStack<int>> _moonWeights;
    
    public override void Register(CRMod mod, EnemyData data)
    {
        if (string.IsNullOrEmpty(data.weatherMultipliers))
        {
            data.weatherMultipliers = "None:1";
        }

        Config = CreateEnemyConfig(mod, data, EnemyType.enemyName);
        
        List<string> weatherMultipliersList = Config.WeatherMultipliers.Value.Split(',').ToList();
        foreach (var weatherMultiplierInList in weatherMultipliersList.Select(s => s.Split(':')))
        {
            string weatherName = weatherMultiplierInList[0].Trim();
            if (weatherMultiplierInList.Count() == 2 && float.TryParse(weatherMultiplierInList[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float multiplier))
            {
                WeatherMultipliers.Add(weatherName, multiplier);
            }
            else
            {
                CodeRebirthLibPlugin.Logger.LogError($"Weather: {weatherName} given invalid or empty multiplier");
            }
        }
        
        EnemyType enemy = EnemyType;
        enemy.MaxCount = Config.MaxSpawnCount.Value;
        enemy.PowerLevel = Config.PowerLevel.Value;
        (Dictionary<Levels.LevelTypes, int> spawnRateByLevelType, Dictionary<string, int> spawnRateByCustomLevelType) = ConfigManager.ParseMoonsWithRarity(Config.SpawnWeights.Value);
        LethalLib.Modules.Enemies.RegisterEnemy(enemy, spawnRateByLevelType, spawnRateByCustomLevelType, TerminalNode, TerminalKeyword);
        mod.EnemyRegistry().Register(this);
    }

    // todo: i dont like how nested this is lmao
    internal static void CreateMoonAttributeStacks()
    {
        foreach (SelectableLevel moon in StartOfRound.Instance.levels)
        {
            foreach (SpawnableEnemyWithRarity enemyWithRarity in moon.Enemies)
            {
                EnemyType enemy = enemyWithRarity.enemyType;
                if (enemy.TryGetDefinition(out CREnemyDefinition? definition) && !definition._moonWeights.ContainsKey(moon))
                {
                    AttributeStack<int> stack = new AttributeStack<int>(enemyWithRarity.rarity);
                    stack.Add(input => { // Handle Weather Multipliers, note that this keeps the reference of 'definition' and 'moon' from the foreach loops
                        string weatherName = moon.currentWeather.ToString();
                        if (!definition.WeatherMultipliers.TryGetValue(weatherName, out float multiplier))
                            return input;
                        
                        return Mathf.FloorToInt(multiplier * input);
                    });
                    definition._moonWeights[moon] = stack;
                }
            }
        }
    }

    internal static void UpdateAllWeights()
    {
        foreach (SelectableLevel moon in StartOfRound.Instance.levels)
        {
            foreach (SpawnableEnemyWithRarity enemyWithRarity in moon.Enemies)
            {
                EnemyType enemy = enemyWithRarity.enemyType;
                if (enemy.TryGetDefinition(out CREnemyDefinition? definition) && !definition._moonWeights.ContainsKey(moon))
                {
                    enemyWithRarity.rarity = definition._moonWeights[moon].Calculate(forceRecalculate: true);
                }
            }
        }
    }
    
    public static EnemyConfig CreateEnemyConfig(CRMod mod, EnemyData data, string enemyName)
    {
        using(ConfigContext section = mod.ConfigManager.CreateConfigSection(enemyName))
        {
            return new EnemyConfig
            {
                SpawnWeights = section.Bind("Spawn Weights", $"Spawn weights for {enemyName}.", data.spawnWeights),
                WeatherMultipliers = section.Bind("Weather Multipliers", $"Weather * SpawnWeight multipliers for {enemyName}.", data.weatherMultipliers),
                PowerLevel = section.Bind("Power Level", $"Power level for {enemyName}.", data.powerLevel),
                MaxSpawnCount = section.Bind("Max Spaw nCount", $"Max spawn count for {enemyName}.", data.maxSpawnCount)
            };
        }
    }

    public const string REGISTRY_ID = "enemies";

    public static void RegisterTo(CRMod mod)
    {
        mod.CreateRegistry(REGISTRY_ID, new CRRegistry<CREnemyDefinition>());
    }
    
    public override List<EnemyData> GetEntities(CRMod mod) => mod.Content.assetBundles.SelectMany(it => it.enemies).ToList(); // probably should be cached but i dont care anymore.
}