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

        if (relevantMoonInfo == null && relevantDungeonInfo == null)
        {
            return $"No moons or interiors found with the user input '{userInput}'.\n\n";
        }

        int spaceForName = 20;
        StringBuilder builder = new StringBuilder();
        if (relevantMoonInfo != null)
        {
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
            BuildMoonSimulation(builder, relevantMoonInfo, includeScrap, includeEnemies, spaceForName);
        }

        if (relevantDungeonInfo != null)
        {
            BuildDungeonSimulation(builder, relevantDungeonInfo, spaceForName);
        }

        return builder.ToString();
    }

    private static void BuildMoonSimulation(StringBuilder builder, DawnMoonInfo moonInfo, bool includeScrap, bool includeEnemies, int spaceForName)
    {
        builder.Append($"Simulating arrival to {moonInfo.Level.PlanetName}\nAnalyzing potential remnants found on surface.\nChecking the Weather forecast.\nListing generated probabilities below.\n\n");

        BuildStructureInfo(builder, moonInfo, spaceForName);
        if (includeScrap)
        {
            BuildScrapsInfo(builder, moonInfo, spaceForName);
        }
        if (includeEnemies)
        {
            BuildEnemiesInfo(builder, moonInfo, spaceForName);
        }
    }

    private static void BuildEnemiesInfo(StringBuilder builder, DawnMoonInfo moonInfo, int spaceForName)
    {
        builder.Append($"----------------------------\n\n");
        builder.Append($"POSSIBLE ENEMIES:\n");

        List<DawnEnemyInfo> possibleEnemies = new();
        List<float> possibleEnemyWeights = new();

        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            int rarity;
            if (enemyInfo.EnemyType.spawnFromWeeds)
            {
                rarity = enemyInfo.Weed.GetRarity(moonInfo, null, null);
            }
            else if (enemyInfo.EnemyType.isDaytimeEnemy)
            {
                rarity = enemyInfo.Daytime.GetRarity(moonInfo, null, null);
            }
            else if (enemyInfo.EnemyType.isOutsideEnemy)
            {
                rarity = enemyInfo.Outside.GetRarity(moonInfo, null, null);
            }
            else
            {
                rarity = enemyInfo.Inside.GetRarity(moonInfo, null, null);
            }

            if (rarity > 0)
            {
                possibleEnemies.Add(enemyInfo);
                possibleEnemyWeights.Add(rarity);
            }
        }

        // sort by rarity
        possibleEnemies.SortWithWeight(possibleEnemyWeights);
        float sumOfEnemyWeights = possibleEnemyWeights.Sum();
        for (int i = 0; i < possibleEnemies.Count; i++)
        {
            string enemyName = possibleEnemies[i].EnemyType.enemyName;
            if (enemyName.Length > 16)
            {
                enemyName = enemyName[..13] + "...";
            }
            int paddingNeeded = Mathf.Max(spaceForName - enemyName.Length, 0);
            builder.Append($"* {enemyName}{new string(' ', paddingNeeded)}");
            builder.Append($"// Chance: ");
            float percentileWeight = (possibleEnemyWeights[i] / sumOfEnemyWeights) * 100f;
            if (percentileWeight < 10f)
            {
                builder.Append(" ");
            }
            builder.Append($"{percentileWeight:F2}% ({possibleEnemyWeights[i]})\n");
        }
    }

    private static void BuildScrapsInfo(StringBuilder builder, DawnMoonInfo moonInfo, int spaceForName)
    {
        builder.Append($"----------------------------\n\n");
        builder.Append($"POSSIBLE ITEMS:\n");

        List<DawnItemInfo> possibleItems = new();
        List<float> possibleItemWeights = new();

        foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
        {
            if (itemInfo.ScrapInfo == null)
            {
                continue;
            }

            int rarity = itemInfo.ScrapInfo.GetRarity(moonInfo, null, null);
            if (rarity > 0)
            {
                possibleItems.Add(itemInfo);
                possibleItemWeights.Add(rarity);
            }
        }

        // sort by rarity
        possibleItems.SortWithWeight(possibleItemWeights);
        float sumOfItemWeights = possibleItemWeights.Sum();
        for (int i = 0; i < possibleItems.Count; i++)
        {
            string itemName = possibleItems[i].Item.itemName;
            if (itemName.Length > 16)
            {
                itemName = itemName[..13] + "...";
            }
            int paddingNeeded = Mathf.Max(spaceForName - itemName.Length, 0);
            builder.Append($"* {itemName}{new string(' ', paddingNeeded)}");
            builder.Append($"// Chance: ");
            float percentileWeight = (possibleItemWeights[i] / sumOfItemWeights) * 100f;
            if (percentileWeight < 10f)
            {
                builder.Append(" ");
            }
            builder.Append($"{percentileWeight:F2}% ({possibleItemWeights[i]})\n");
        }
    }

    private static void BuildStructureInfo(StringBuilder builder, DawnMoonInfo moonInfo, int spaceForName)
    {
        builder.Append($"----------------------------\n\n");
        builder.Append($"POSSIBLE STRUCTURES:\n");
        List<DawnDungeonInfo> possibleDungeons = new();
        List<float> possibleDungeonWeights = new();

        foreach (DawnDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
        {
            int rarity = dungeonInfo.GetRarity(moonInfo, null);
            if (rarity > 0)
            {
                possibleDungeons.Add(dungeonInfo);
                possibleDungeonWeights.Add(rarity);
            }
        }

        // sort by rarity
        possibleDungeons.SortWithWeight(possibleDungeonWeights);
        float sumOfDungeonWeights = possibleDungeonWeights.Sum();
        for (int i = 0; i < possibleDungeons.Count; i++)
        {
            string dungeonName = possibleDungeons[i].GetPublicName();
            if (dungeonName.Length > 16)
            {
                dungeonName = dungeonName[..13] + "...";
            }
            int paddingNeeded = Mathf.Max(spaceForName - dungeonName.Length, 0);
            builder.Append($"* {dungeonName}{new string(' ', paddingNeeded)}");
            builder.Append($"// Chance: ");
            float percentileWeight = (possibleDungeonWeights[i] / sumOfDungeonWeights) * 100f;
            if (percentileWeight < 10f)
            {
                builder.Append(" ");
            }
            builder.Append($"{percentileWeight:F2}% ({possibleDungeonWeights[i]})\n");
        }
    }

    private static void BuildDungeonSimulation(StringBuilder builder, DawnDungeonInfo dungeonInfo, int spaceForName)
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

            float rarityWithThisDungeon = dungeonInfo.GetRarity(moonInfo, null);
            if (rarityWithThisDungeon <= 0)
            {
                continue;
            }

            float sumOfWeightsOfAllDungeons = LethalContent.Dungeons.Values.Sum(d => d.GetRarity(moonInfo, null));
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