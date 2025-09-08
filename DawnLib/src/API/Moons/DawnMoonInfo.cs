using System;
using System.Collections.Generic;
using System.Globalization;

namespace Dawn;

public class DawnMoonInfo : DawnBaseInfo<DawnMoonInfo>, ITerminalPurchase
{
    internal DawnMoonInfo(NamespacedKey<DawnMoonInfo> key, HashSet<NamespacedKey> tags, SelectableLevel level, TerminalNode? routeNode, TerminalKeyword? nameKeyword, IDataContainer? customData) : base(key, tags, customData)
    {
        Level = level;
        RouteNode = routeNode;
        NameKeyword = nameKeyword;

        if (routeNode == null)
        {
            Cost = new SimpleProvider<int>(-1);
            PurchasePredicate = ITerminalPurchasePredicate.AlwaysHide();
        }
        else
        {
            Cost = new SimpleProvider<int>(routeNode.itemCost);
            PurchasePredicate = ITerminalPurchasePredicate.AlwaysSuccess();
        }
    }

    public SelectableLevel Level { get; }
    public TerminalNode? RouteNode { get; }
    public TerminalKeyword? NameKeyword { get; }

    public DawnWeatherEffectInfo? GetCurrentWeather()
    {
        LevelWeatherType type = Level.currentWeather;
        if (type == LevelWeatherType.None)
        {
            return null;
        }

        WeatherEffect effect = TimeOfDay.Instance.effects[(int)type];
        if (effect.HasDawnInfo())
        {
            return effect.GetDawnInfo();
        }
        return null;
    }
    
    public string GetConfigName()
    {
        // -> 10-Example
        string newName = StripSpecialCharacters(Level.PlanetName);
        // -> Example
        if (!newName.EndsWith("Level", true, CultureInfo.InvariantCulture))
            newName += "Level";
        // -> ExampleLevel
        newName = newName.ToLowerInvariant();
        // -> examplelevel
        return newName;
    }

    public void RouteTo()
    {
        Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>(); // todo :p
        int index = Array.IndexOf(StartOfRound.Instance.levels, Level);
        
        StartOfRound.Instance.ChangeLevelServerRpc(index, terminal.groupCredits);
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
    
    public IProvider<int> Cost { get; }
    public ITerminalPurchasePredicate PurchasePredicate { get; }
}