using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dawn.Internal;
using Dawn.Utils;
using UnityEngine;

namespace Dawn;

public static class DawnCommands
{
    public static void Init()
    {
        CreateSimulateCommand();
        CreateFilterCommand();
    }

    private static void CreateSimulateCommand()
    {
        TerminalCommandBasicInformation inputCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibSimulate", "DawnCommand", "Takes the player's input, checks if there's any moons or interiors with that name and simulates their weights.", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "simulate_command"), inputCommandBasicInformation, builder =>
        {
            builder.SetKeywords(["simulate"]);
            builder.DefineInputCommand(inputBuilder =>
            {
                inputBuilder.SetResultDisplayText(SimulateCommand);
            });
        });
    }

    private static string SimulateCommand(string userInput)
    {
        if (RoundManagerRefs.Instance == null)
        {
            return "RoundManager not initialized yet.\n\n";
        }

        DawnMoonInfo? relevantMoonInfo = RoundManagerRefs.Instance.currentLevel.DawnInfo;
        if (!string.IsNullOrEmpty(userInput))
        {
            relevantMoonInfo = null;
            foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
            {
                if (moonInfo.GetNumberlessPlanetName().StartsWith(userInput, StringComparison.OrdinalIgnoreCase))
                {
                    relevantMoonInfo = moonInfo;
                    break;
                }
            }
        }

        DawnDungeonInfo? relevantDungeonInfo = null;
        if (!string.IsNullOrEmpty(userInput))
        {
            foreach (DawnDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
            {
                if (dungeonInfo.GetPublicName().StartsWith(userInput, StringComparison.OrdinalIgnoreCase))
                {
                    relevantDungeonInfo = dungeonInfo;
                    break;
                }
            }
        }

        DawnWeatherEffectInfo? relevantWeatherEffectInfo = null;
        if (!string.IsNullOrEmpty(userInput))
        {
            foreach (DawnWeatherEffectInfo weatherEffectInfo in LethalContent.Weathers.Values)
            {
                if (weatherEffectInfo.GetLevelWeatherEffect().ToString().StartsWith(userInput, StringComparison.OrdinalIgnoreCase))
                {
                    relevantWeatherEffectInfo = weatherEffectInfo;
                    break;
                }
            }
        }

        if (relevantMoonInfo == null && relevantDungeonInfo == null && relevantWeatherEffectInfo == null)
        {
            return $"No moons or interiors or weathers found with the user input '{userInput}'.\n\n";
        }

        bool includeScrap = true;
        if (userInput.Contains("-s", StringComparison.OrdinalIgnoreCase))
        {
            includeScrap = false;
        }

        bool includeEnemies = true;
        if (userInput.Contains("-e", StringComparison.OrdinalIgnoreCase))
        {
            includeEnemies = false;
        }

        int spaceForName = 20;
        StringBuilder builder = new StringBuilder();
        if (relevantMoonInfo != null)
        {
            BuildMoonSimulation(builder, relevantMoonInfo, includeScrap, includeEnemies, spaceForName);
        }

        if (relevantDungeonInfo != null)
        {
            BuildDungeonSimulation(builder, relevantDungeonInfo, includeScrap, includeEnemies, spaceForName);
        }

        if (relevantWeatherEffectInfo != null)
        {
            // BuildWeatherSimulation(builder, relevantWeatherEffectInfo, includeScrap, includeEnemies, spaceForName);
        }

        return builder.ToString();
    }

    private static List<string> _funnyComments = new()
    {
        "Making a Coffee...",
        "Cooking a Maneater.",
        "Staring into a Bracken's eyes.",
        "Buying 67 Shovels.",
        "Baking a Cake.",
        "Escaping the Matrix."
    };

    private static void BuildMoonSimulation(StringBuilder builder, DawnMoonInfo moonInfo, bool includeScrap, bool includeEnemies, int spaceForName)
    {
        builder.Append($"Simulating arrival to {moonInfo.Level.PlanetName}\nAnalyzing potential remnants found on surface.\n{_funnyComments[UnityEngine.Random.Range(0, _funnyComments.Count)]}\nListing generated probabilities below.\n\n");

        BuildStructureInfo(builder, moonInfo, null, spaceForName);
        if (includeScrap)
        {
            BuildScrapsInfo(builder, moonInfo, null, null, spaceForName);
        }
        if (includeEnemies)
        {
            BuildEnemiesInfo(builder, moonInfo, null, null, spaceForName);
        }
    }

    private static void BuildEnemiesInfo(StringBuilder builder, DawnMoonInfo? moonInfo, DawnDungeonInfo? dungeonInfo, DawnWeatherEffectInfo? weatherEffectInfo, int spaceForName)
    {
        builder.Append("----------------------------\n\n");
        builder.Append("POSSIBLE ENEMIES:\n");
        IEnumerable<DawnEnemyInfo> enemyInfos = LethalContent.Enemies.Values;
        int count = 0;
        count += BuildWeightedInfo(builder, enemyInfos.Where(enemyInfo => enemyInfo.EnemyType.spawnFromWeeds), enemyInfo => enemyInfo.Weed.GetRarity(moonInfo, dungeonInfo, weatherEffectInfo, false), enemyInfo => enemyInfo.EnemyType.enemyName, spaceForName, "WEED");
        count += BuildWeightedInfo(builder, enemyInfos.Where(enemyInfo => !enemyInfo.EnemyType.spawnFromWeeds && enemyInfo.EnemyType.isDaytimeEnemy), enemyInfo => enemyInfo.Daytime.GetRarity(moonInfo, dungeonInfo, weatherEffectInfo, false), enemyInfo => enemyInfo.EnemyType.enemyName, spaceForName, "DAYTIME");
        count += BuildWeightedInfo(builder, enemyInfos.Where(enemyInfo => !enemyInfo.EnemyType.spawnFromWeeds && !enemyInfo.EnemyType.isDaytimeEnemy && enemyInfo.EnemyType.isOutsideEnemy), enemyInfo => enemyInfo.Outside.GetRarity(moonInfo, dungeonInfo, weatherEffectInfo, false), enemyInfo => enemyInfo.EnemyType.enemyName, spaceForName, "OUTSIDE");
        count += BuildWeightedInfo(builder, enemyInfos.Where(enemyInfo => !enemyInfo.EnemyType.spawnFromWeeds && !enemyInfo.EnemyType.isDaytimeEnemy && !enemyInfo.EnemyType.isOutsideEnemy), enemyInfo => enemyInfo.Inside.GetRarity(moonInfo, dungeonInfo, weatherEffectInfo, false), enemyInfo => enemyInfo.EnemyType.enemyName, spaceForName, "INSIDE");
        if (count <= 0)
        {
            builder.Append($"No Enemies found.\n\n");
        }
    }

    private static void BuildScrapsInfo(StringBuilder builder, DawnMoonInfo? moonInfo, DawnDungeonInfo? dungeonInfo, DawnWeatherEffectInfo? weatherEffectInfo, int spaceForName)
    {
        builder.Append("----------------------------\n\n");
        builder.Append("POSSIBLE ITEMS:\n");
        int count = BuildWeightedInfo(builder, LethalContent.Items.Values.Where(itemInfo => itemInfo.ScrapInfo != null), itemInfo => itemInfo.ScrapInfo!.GetRarity(moonInfo, dungeonInfo, weatherEffectInfo, false), itemInfo => itemInfo.Item.itemName, spaceForName);
        if (count <= 0)
        {
            builder.Append($"No Scraps found.\n\n");
        }
    }

    private static void BuildStructureInfo(StringBuilder builder, DawnMoonInfo? moonInfo, DawnWeatherEffectInfo? weatherEffectInfo, int spaceForName)
    {
        builder.Append($"----------------------------\n\n");
        builder.Append($"POSSIBLE STRUCTURES:\n");
        int count = BuildWeightedInfo(builder, LethalContent.Dungeons.Values, dungeonInfo => dungeonInfo.GetRarity(moonInfo, weatherEffectInfo, false), dungeonInfo => dungeonInfo.GetPublicName(), spaceForName);
        if (count <= 0)
        {
            builder.Append($"No Structures found.\n\n");
        }
    }

    private static void BuildDungeonSimulation(StringBuilder builder, DawnDungeonInfo dungeonInfo, bool includeScrap, bool includeEnemies, int spaceForName)
    {
        string dungeonName = dungeonInfo.GetPublicName();
        builder.Append($"Simulating the structure {dungeonName}\nAnalyzing the pathways of the structure.\nChecking the Weather forecast.\nListing generated probabilities below.\n\n");
        builder.Append($"----------------------------\n\n");
        builder.Append($"POSSIBLE MOONS:\n");
        List<DawnMoonInfo> possibleMoons = new();
        List<float> possibleMoonWeights = new();

        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.HasTag(Tags.Unimplemented))
            {
                continue;
            }

            float rarityWithThisDungeon = dungeonInfo.GetRarity(moonInfo, null, false);
            if (rarityWithThisDungeon <= 0)
            {
                continue;
            }

            float sumOfWeightsOfAllDungeons = LethalContent.Dungeons.Values.Sum(d => d.GetRarity(moonInfo, null, false));
            float rarity = (rarityWithThisDungeon / sumOfWeightsOfAllDungeons) * 100f;

            possibleMoons.Add(moonInfo);
            possibleMoonWeights.Add(rarity);
        }

        // sort by rarity
        possibleMoons.SortWithWeight(possibleMoonWeights);
        for (int i = 0; i < possibleMoons.Count; i++)
        {
            string moonName = possibleMoons[i].GetNumberlessPlanetName();
            int paddingNeeded = Mathf.Max(spaceForName - moonName.Length, 0);
            builder.Append($"* {moonName}{new string(' ', paddingNeeded)}");
            builder.Append($"// Chance: ");
            float percentileWeight = possibleMoonWeights[i];
            if (percentileWeight < 10f)
            {
                builder.Append(" ");
            }
            builder.Append($"{percentileWeight:F2}%\n");
        }

        if (includeScrap)
        {
            BuildScrapsInfo(builder, null, dungeonInfo, null, spaceForName);
        }

        if (includeEnemies)
        {
            BuildEnemiesInfo(builder, null, dungeonInfo, null, spaceForName);
        }
    }

    private static int BuildWeightedInfo<T>(StringBuilder builder, IEnumerable<T> values, Func<T, float> weightSelector, Func<T, string> nameSelector, int spaceForName, string? subsectionName = null)
    {
        List<T> possibleValues = new();
        List<float> possibleWeights = new();

        foreach (T value in values)
        {
            float weight = weightSelector(value);
            if (weight <= 0)
            {
                continue;
            }

            possibleValues.Add(value);
            possibleWeights.Add(weight);
        }

        if (possibleValues.Count == 0)
        {
            return 0;
        }

        if (subsectionName != null)
        {
            builder.Append($"\n{subsectionName}:\n");
        }

        possibleValues.SortWithWeight(possibleWeights);

        float sumOfWeights = possibleWeights.Sum();

        for (int i = 0; i < possibleValues.Count; i++)
        {
            string name = nameSelector(possibleValues[i]);

            if (name.Length > 16)
            {
                name = name[..13] + "...";
            }

            int paddingNeeded = Mathf.Max(spaceForName - name.Length, 0);

            builder.Append($"* {name}{new string(' ', paddingNeeded)}");
            builder.Append("// Chance: ");

            float percentileWeight = (possibleWeights[i] / sumOfWeights) * 100f;

            if (percentileWeight < 10f)
            {
                builder.Append(" ");
            }

            builder.Append($"{percentileWeight:F2}% ({possibleWeights[i]})\n");
        }

        return possibleValues.Count;
    }

    private static void CreateFilterCommand()
    {
        TerminalCommandBasicInformation inputCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibFilter", "DawnCommand", "Takes the player's input, filters the moon list for that tag.", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "filter_command"), inputCommandBasicInformation, builder =>
        {
            builder.SetKeywords(["filter"]);
            builder.DefineInputCommand(inputBuilder =>
            {
                inputBuilder.SetResultDisplayText(FilterCommand);
            });
        });
    }

    private static string FilterCommand(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            return "Please enter a valid tag to filter for (type `filter none` or `filter reset` to remove all filters).\n\n";
        }

        if (userInput.Equals("none", StringComparison.OrdinalIgnoreCase) || userInput.Equals("reset", StringComparison.OrdinalIgnoreCase))
        {
            MoonRegistrationHandler.MoonGroupAlgorithm.FilterSteps.Clear();
            MoonRegistrationHandler.MoonGroupAlgorithm.FilterSteps.Add(new VisibleFilterStep());
            return $"Removed Filters.\n\n";
        }

        HashSet<NamespacedKey> tags = new();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            foreach (NamespacedKey tag in moonInfo.AllTags())
            {
                if (tag.Key.StartsWith(userInput, StringComparison.OrdinalIgnoreCase))
                {
                    tags.Add(tag);
                }
            }
        }

        if (tags.Count == 0)
        {
            return "Please enter a valid tag to filter for (type `filter none` or `filter reset` to remove all filters).\n\n";
        }

        foreach (NamespacedKey tag in tags)
        {
            MoonRegistrationHandler.MoonGroupAlgorithm.FilterSteps.Add(new TagFilterStep(tag));
        }

        string tagsString = string.Join(", ", tags.Select(tag => tag.Key).OrderBy(tag => tag).ToArray());
        string plural = tags.Count == 1 ? string.Empty : "s";
        return $"Filtering for the following tag{plural}: {tagsString}\n\n";
    }
}