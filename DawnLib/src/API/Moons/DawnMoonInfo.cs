using System;
using System.Collections.Generic;
using System.Globalization;
using Dawn.Internal;

namespace Dawn;

public class DawnMoonInfo : DawnBaseInfo<DawnMoonInfo>
{
    internal DawnMoonInfo(NamespacedKey<DawnMoonInfo> key, HashSet<NamespacedKey> tags, SelectableLevel level, float outsideEnemiesProbabilityRange, List<IMoonSceneInfo> scenes, TerminalNode? routeNode, TerminalNode? receiptNode, TerminalKeyword? nameKeyword, DawnPurchaseInfo dawnPurchaseInfo, IDataContainer? customData) : base(key, tags, customData)
    {
        Level = level;

        OutsideEnemiesProbabilityRange = outsideEnemiesProbabilityRange;

        Scenes.AddRange(scenes);

        RouteNode = routeNode;
        ReceiptNode = receiptNode;
        NameKeyword = nameKeyword;

        DawnPurchaseInfo = dawnPurchaseInfo;
    }

    public SelectableLevel Level { get; }
    public TerminalNode? RouteNode { get; }
    public TerminalNode? ReceiptNode { get; }
    public TerminalKeyword? NameKeyword { get; }
    public float OutsideEnemiesProbabilityRange { get; private set; }

    public List<IMoonSceneInfo> Scenes { get; } = [];

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
        {
            newName += "Level";
        }
        // -> ExampleLevel
        newName = newName.ToLowerInvariant();
        // -> examplelevel
        return newName;
    }

    public string GetNumberlessPlanetName() => StripSpecialCharacters(Level.PlanetName);

    public void RouteTo()
    {
        int index = Array.IndexOf(StartOfRound.Instance.levels, Level);
        StartOfRound.Instance.ChangeLevelServerRpc(index, TerminalRefs.Instance.groupCredits);
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

    public DawnPurchaseInfo DawnPurchaseInfo { get; }
}