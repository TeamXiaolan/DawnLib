using System.Collections.Generic;
using Dawn.Utils;
using UnityEngine;

namespace Dawn;
public class MoonInfoBuilder : BaseInfoBuilder<DawnMoonInfo, SelectableLevel, MoonInfoBuilder>
{
    private TerminalNode? _routeNode, _receiptNode;
    private TerminalKeyword? _nameKeyword;
    private List<IMoonSceneInfo> _scenes = [];

    private IProvider<int>? _costOverride;
    private ITerminalPurchasePredicate? _purchasePredicate;
    private float _outsideEnemiesProbabilityRange = 3f;

    public MoonInfoBuilder(NamespacedKey<DawnMoonInfo> key, SelectableLevel value) : base(key, value)
    {
    }

    public MoonInfoBuilder OverrideRouteNode(TerminalNode node)
    {
        _routeNode = node;
        return this;
    }

    public MoonInfoBuilder CreateNameKeyword(string wordOverride)
    {
        if (string.IsNullOrWhiteSpace(wordOverride))
        {
            wordOverride = value.PlanetName.ToLowerInvariant().Replace(' ', '-');
        }

        _nameKeyword = new TerminalKeywordBuilder($"{value.PlanetName}NameKeyword", wordOverride, ITerminalKeyword.DawnKeywordType.Moons)
            .Build();

        _nameKeyword.compatibleNouns = [];

        return this;
    }

    public MoonInfoBuilder AddScene(NamespacedKey<IMoonSceneInfo> sceneKey, AnimationClip shipLandingOverrideAnimation, AnimationClip shipTakeoffOverrideAnimation, ProviderTable<int?, DawnMoonInfo> weight, string assetBundlePath, string scenePath)
    {
        _scenes.Add(new CustomMoonSceneInfo(sceneKey, shipLandingOverrideAnimation, shipTakeoffOverrideAnimation, weight, assetBundlePath, scenePath));
        return this;
    }

    private static string StripSpecialCharacters(string input)
    {
        string returnString = string.Empty;
        foreach (char charmander in input)
        {
            if (!char.IsLetter(charmander))
            {
                continue;
            }

            returnString += charmander;
        }
        return returnString;
    }

    public MoonInfoBuilder OverrideCost(int cost)
    {
        return OverrideCost(new SimpleProvider<int>(cost));
    }

    public MoonInfoBuilder OverrideTimeMultiplier(float multiplier)
    {
        value.DaySpeedMultiplier = multiplier;
        return this;
    }

    public MoonInfoBuilder OverrideMinMaxScrap(BoundedRange range)
    {
        value.minScrap = (int)range.Min;
        value.maxScrap = (int)range.Max;
        return this;
    }

    public MoonInfoBuilder OverrideEnemyPowerCount(int maxInsidePowerCount, int maxOutsidePowerCount, int maxDaytimePowerCount)
    {
        value.maxEnemyPowerCount = maxInsidePowerCount;
        value.maxOutsideEnemyPowerCount = maxOutsidePowerCount;
        value.maxDaytimeEnemyPowerCount = maxDaytimePowerCount;
        return this;
    }

    public MoonInfoBuilder OverrideEnemySpawnCurves(AnimationCurve insideAnimationCurve, AnimationCurve outsideAnimationCurve, AnimationCurve daytimeAnimationCurve)
    {
        value.enemySpawnChanceThroughoutDay = insideAnimationCurve;
        value.outsideEnemySpawnChanceThroughDay = outsideAnimationCurve;
        value.daytimeEnemySpawnChanceThroughDay = daytimeAnimationCurve;
        return this;
    }

    public MoonInfoBuilder OverrideEnemySpawnRanges(float insideSpawnRange, float outsideSpawnRange, float daytimeSpawnRange)
    {
        value.spawnProbabilityRange = insideSpawnRange;
        _outsideEnemiesProbabilityRange = outsideSpawnRange;
        value.daytimeEnemiesProbabilityRange = daytimeSpawnRange;
        return this;
    }

    public MoonInfoBuilder OverrideCost(IProvider<int> cost)
    {
        _costOverride = cost;
        return this;
    }

    public MoonInfoBuilder SetPurchasePredicate(ITerminalPurchasePredicate predicate)
    {
        _purchasePredicate = predicate;
        return this;
    }

    override internal DawnMoonInfo Build()
    {
        if (_routeNode == null)
        {
            _routeNode = new TerminalNodeBuilder($"{value.PlanetName}RouteNode")
                .SetDisplayText($"The cost to route to {value.PlanetName} is [totalCost]. It is \ncurrently [currentPlanetTime] on this moon.\n\nPlease CONFIRM or DENY.\n\n")
                .SetClearPreviousText(true)
                .SetMaxCharactersToType(25)
                .Build();
        }

        if (_receiptNode == null)
        {
            _receiptNode = new TerminalNodeBuilder($"{value.PlanetName}RecieptNode")
                .SetDisplayText($"Routing autopilot to {value.PlanetName}.\nYour new balance is [playerCredits].\n\nPlease enjoy your flight.\n\n")
                .SetClearPreviousText(true)
                .SetMaxCharactersToType(25)
                .Build();
        }

        if (_nameKeyword == null)
        {
            CreateNameKeyword(StripSpecialCharacters(value.PlanetName).ToLowerInvariant());
        }

        if (_scenes.Count == 0)
        {
            DawnPlugin.Logger.LogError($"Moon: '{key}' has 0 scenes.");
        }

        _purchasePredicate ??= ITerminalPurchasePredicate.AlwaysSuccess();
        _costOverride ??= new SimpleProvider<int>(_routeNode.itemCost);

        DawnMoonInfo info = new DawnMoonInfo(key, tags, value, _outsideEnemiesProbabilityRange, _scenes, _routeNode, _receiptNode, _nameKeyword, new DawnPurchaseInfo(_costOverride, _purchasePredicate), customData);
        return info;
    }
}