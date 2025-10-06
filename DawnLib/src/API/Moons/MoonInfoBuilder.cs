using System.Collections.Generic;

namespace Dawn;
public class MoonInfoBuilder : BaseInfoBuilder<DawnMoonInfo, SelectableLevel, MoonInfoBuilder>
{
    private TerminalNode? _routeNode, _receiptNode;
    private TerminalKeyword? _nameKeyword;
    private List<IMoonSceneInfo> _scenes = [];

    private IProvider<int>? _costOverride;
    private ITerminalPurchasePredicate? _purchasePredicate;

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
        if (string.IsNullOrEmpty(wordOverride))
        {
            wordOverride = value.PlanetName.ToLowerInvariant().Replace(' ', '-');
        }

        _nameKeyword = new TerminalKeywordBuilder($"{value.PlanetName}NameKeyword")
            .SetWord(wordOverride)
            .Build();

        _nameKeyword.compatibleNouns = [];

        return this;
    }

    public MoonInfoBuilder AddScene(NamespacedKey<IMoonSceneInfo> sceneKey, int weight, string assetBundlePath, string scenePath)
    {
        _scenes.Add(new CustomMoonSceneInfo(sceneKey, new SimpleProvider<int>(weight), assetBundlePath, scenePath));
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

        DawnMoonInfo info = new DawnMoonInfo(key, tags, value, _scenes, _routeNode, _receiptNode, _nameKeyword, _costOverride, _purchasePredicate, customData);
        return info;
    }
}