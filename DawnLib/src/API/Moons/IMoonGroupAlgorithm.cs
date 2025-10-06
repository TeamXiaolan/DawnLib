using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Utils;
using UnityEngine.InputSystem.Utilities;

namespace Dawn;

public class MoonGroup
{
    public string GroupName = "";
    public List<DawnMoonInfo> Moons = [];
}

public interface IMoonGroupAlgorithm
{
    List<MoonGroup> Group(IEnumerable<DawnMoonInfo> input);
}

public class FixedGroupSizeAlgorithm(int groupSize = 3) : IMoonGroupAlgorithm
{
    public List<MoonGroup> Group(IEnumerable<DawnMoonInfo> input)
    {
        MoonGroup companyMoons = new MoonGroup();
        MoonGroup currentGroup = new MoonGroup();
        List<MoonGroup> groups = [currentGroup];

        int currentInGroup = 0;

        foreach (DawnMoonInfo moonInfo in input)
        {
            if (moonInfo.HasTag(Tags.Company))
            {
                companyMoons.Moons.Add(moonInfo);
                continue;
            }

            currentGroup.Moons.Add(moonInfo);

            if (moonInfo.PurchasePredicate.CanPurchase() is not TerminalPurchaseResult.HiddenPurchaseResult)
            {
                currentInGroup++;
            }

            // at 3 moons, start a new group and begin again
            if (currentInGroup == groupSize)
            {
                currentGroup = new MoonGroup();
                currentInGroup = 0;
                groups.Add(currentGroup);
            }
        }

        return [companyMoons, .. groups];
    }
}

public class RankGroupAlgorithm : IMoonGroupAlgorithm
{
    private static readonly string[] _riskIndexes = ["F", "E", "D", "C", "B", "A", "S", "UNKNOWN"];
    private IMoonGroupAlgorithm _subAlgorithm = new FixedGroupSizeAlgorithm();

    public List<MoonGroup> Group(IEnumerable<DawnMoonInfo> input)
    {
        List<MoonGroup> groups = _subAlgorithm.Group(input.OrderBy(RiskLevelComparer));

        foreach (MoonGroup group in groups)
        {
            group.Moons = group.Moons.OrderBy(it => LethalContent.Moons.Values.IndexOf(it)).ToList();
        }

        return groups;
    }

    int RiskLevelComparer(DawnMoonInfo moonInfo)
    {
        string riskLevel = moonInfo.Level.riskLevel ?? "UNKNOWN";
        riskLevel = riskLevel.StripSpecialCharacters();

        return Array.IndexOf(_riskIndexes, riskLevel);
    }
}