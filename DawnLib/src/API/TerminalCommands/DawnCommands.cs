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
        if (string.IsNullOrEmpty(userInput))
        {
            return "Please enter a moon or interior name.\n\n";
        }

        if (userInput.Length <= 2)
        {
            return "Please enter a name for an interior or moon longer than 2 characters.\n\n";
        }

        DawnMoonInfo? relevantMoonInfo = null;
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.GetNumberlessPlanetName().StartsWith(userInput, StringComparison.OrdinalIgnoreCase))
            {
                relevantMoonInfo = moonInfo;
                break;
            }
        }

        DawnDungeonInfo? relevantDungeonInfo = null;
        foreach (DawnDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
        {
            if (dungeonInfo.Key.Key.StartsWith(userInput, StringComparison.OrdinalIgnoreCase))
            {
                relevantDungeonInfo = dungeonInfo;
                break;
            }
        }

        if (relevantMoonInfo == null && relevantDungeonInfo == null)
        {
            return $"No moons or interiors found with the user input '{userInput}'.\n\n";
        }

        bool includeWeather = true;
        if (userInput.EndsWith("-w"))
        {
            includeWeather = false;
        }

        int spaceForName = 20;
        StringBuilder builder = new StringBuilder();
        if (relevantMoonInfo != null)
        {
            builder.Append($"Simulating arrival to {relevantMoonInfo.Level.PlanetName}\nAnalyzing potential remnants found on surface.\nChecking the Weather forecast.\nListing generated probabilities below.\n\n");
            builder.Append($"----------------------------\n\n");
            builder.Append($"POSSIBLE STRUCTURES:\n");
            List<DawnDungeonInfo> possibleDungeons = new();
            List<float> possibleDungeonWeights = new();

            foreach (DawnDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
            {
                int rarity = dungeonInfo.Weights.GetFor(relevantMoonInfo, new SpawnWeightContext(relevantMoonInfo, null, TimeOfDayRefs.GetCurrentWeatherEffect(relevantMoonInfo.Level)?.GetDawnInfo())) ?? 0;
                if (rarity > 0)
                {
                    possibleDungeons.Add(dungeonInfo);
                    possibleDungeonWeights.Add(rarity);
                }
            }

            // sort by rarity
            for (int i = 0; i < possibleDungeons.Count; i++)
            {
                for (int j = i + 1; j < possibleDungeons.Count; j++)
                {
                    if (possibleDungeonWeights[i] < possibleDungeonWeights[j])
                    {
                        (possibleDungeons[i], possibleDungeons[j]) = (possibleDungeons[j], possibleDungeons[i]);
                        (possibleDungeonWeights[i], possibleDungeonWeights[j]) = (possibleDungeonWeights[j], possibleDungeonWeights[i]);
                    }
                }
            }

            float sumOfWeights = possibleDungeonWeights.Sum();
            for (int i = 0; i < possibleDungeons.Count; i++)
            {
                string dungeonName = possibleDungeons[i].Key.Key.RemoveLeadingNumbers().ToCapitalized().ReplaceNumbersWithWords().Replace(" ", "_");
                int paddingNeeded = Mathf.Max(spaceForName - dungeonName.Length, 0);
                builder.Append($"* {dungeonName}{new string(' ', paddingNeeded)}");
                builder.Append($"// Chance: ");
                float weight = (possibleDungeonWeights[i] / sumOfWeights) * 100f;
                if (weight < 10f)
                {
                    builder.Append(" ");
                }
                builder.Append($"{weight:F2}%\n");
            }
        }

        if (relevantDungeonInfo != null)
        {
            string dungeonName = relevantDungeonInfo.Key.Key.RemoveLeadingNumbers().ToCapitalized().ReplaceNumbersWithWords().Replace(" ", "_");
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

                float rarityWithThisDungeon = relevantDungeonInfo.Weights.GetFor(moonInfo, new SpawnWeightContext(moonInfo, null, TimeOfDayRefs.GetCurrentWeatherEffect(moonInfo.Level)?.GetDawnInfo())) ?? 0;
                if (rarityWithThisDungeon <= 0)
                {
                    continue;
                }

                float sumOfWeightsOfAllDungeons = LethalContent.Dungeons.Values.Sum(d => d.Weights.GetFor(moonInfo, new SpawnWeightContext(moonInfo, null, TimeOfDayRefs.GetCurrentWeatherEffect(moonInfo.Level)?.GetDawnInfo())) ?? 0);
                float rarity = (rarityWithThisDungeon / sumOfWeightsOfAllDungeons) * 100f;

                possibleMoons.Add(moonInfo);
                possibleMoonWeights.Add(rarity);
            }

            // sort by rarity
            for (int i = 0; i < possibleMoons.Count; i++)
            {
                for (int j = i + 1; j < possibleMoons.Count; j++)
                {
                    if (possibleMoonWeights[i] < possibleMoonWeights[j])
                    {
                        (possibleMoons[i], possibleMoons[j]) = (possibleMoons[j], possibleMoons[i]);
                        (possibleMoonWeights[i], possibleMoonWeights[j]) = (possibleMoonWeights[j], possibleMoonWeights[i]);
                    }
                }
            }

            for (int i = 0; i < possibleMoons.Count; i++)
            {
                string moonName = possibleMoons[i].Key.Key.RemoveLeadingNumbers().ToCapitalized().ReplaceNumbersWithWords().Replace(" ", "_");
                int paddingNeeded = Mathf.Max(spaceForName - moonName.Length, 0);
                builder.Append($"* {moonName}{new string(' ', paddingNeeded)}");
                builder.Append($"// Chance: ");
                float weight = possibleMoonWeights[i];
                if (weight < 10f)
                {
                    builder.Append(" ");
                }
                builder.Append($"{weight:F2}%\n");
            }
        }

        return builder.ToString();
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