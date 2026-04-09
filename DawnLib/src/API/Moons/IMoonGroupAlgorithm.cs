using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Utils;

namespace Dawn;

public class MoonGroup
{
    public string GroupName = "";
    public List<DawnMoonInfo> Moons = [];
}

public interface IMoonFilterStep
{
    IEnumerable<DawnMoonInfo> Filter(IEnumerable<DawnMoonInfo> input);
}

public interface IMoonOrderingStep
{
    IOrderedEnumerable<DawnMoonInfo> ApplyInitial(IEnumerable<DawnMoonInfo> input, bool reverse = false);
    IOrderedEnumerable<DawnMoonInfo> ApplyNext(IOrderedEnumerable<DawnMoonInfo> input, bool reverse = false);
}

public interface IMoonGroupingAlgorithm
{
    List<MoonGroup> Group(IEnumerable<DawnMoonInfo> input);
}

#region VisibleFiltering
public class VisibleFilterStep : IMoonFilterStep
{
    public IEnumerable<DawnMoonInfo> Filter(IEnumerable<DawnMoonInfo> input)
    {
        return input.Where(moon => moon.DawnPurchaseInfo.PurchasePredicate.CanPurchase() is not TerminalPurchaseResult.HiddenPurchaseResult);
    }
}
#endregion
#region TagFiltering
public class TagFilterStep(NamespacedKey tag) : IMoonFilterStep
{
    public IEnumerable<DawnMoonInfo> Filter(IEnumerable<DawnMoonInfo> input)
    {
        return input.Where(moon => moon.HasTag(tag));
    }
}
#endregion
#region FixedGroupSizeGrouping
public class FixedGroupSizeGroupingAlgorithm(int groupSize = 3) : IMoonGroupingAlgorithm
{
    public List<MoonGroup> Group(IEnumerable<DawnMoonInfo> input)
    {
        MoonGroup companyMoons = new();
        MoonGroup currentGroup = new();
        List<MoonGroup> groups = [currentGroup];

        int currentVisibleInGroup = 0;

        foreach (DawnMoonInfo moonInfo in input)
        {
            if (moonInfo.HasTag(Tags.Company))
            {
                companyMoons.Moons.Add(moonInfo);
                continue;
            }

            currentGroup.Moons.Add(moonInfo);

            if (moonInfo.DawnPurchaseInfo.PurchasePredicate.CanPurchase() is not TerminalPurchaseResult.HiddenPurchaseResult)
            {
                currentVisibleInGroup++;
            }

            if (currentVisibleInGroup == groupSize)
            {
                currentGroup = new MoonGroup();
                currentVisibleInGroup = 0;
                groups.Add(currentGroup);
            }
        }

        groups.RemoveAll(group => group.Moons.Count == 0);

        if (companyMoons.Moons.Count > 0)
        {
            groups.Insert(0, companyMoons);
        }

        return groups;
    }
}
#endregion
#region RiskBasedOrdering
public class RankOrderingStep : IMoonOrderingStep
{
    private static readonly string[] RiskIndexes = ["F", "E", "D", "C", "B", "A", "S", "UNKNOWN"];

    public IOrderedEnumerable<DawnMoonInfo> ApplyInitial(IEnumerable<DawnMoonInfo> input, bool reverse = false)
    {
        return reverse
            ? input.OrderByDescending(GetRiskIndex)
            : input.OrderBy(GetRiskIndex);
    }

    public IOrderedEnumerable<DawnMoonInfo> ApplyNext(IOrderedEnumerable<DawnMoonInfo> input, bool reverse = false)
    {
        return reverse
            ? input.ThenByDescending(GetRiskIndex)
            : input.ThenBy(GetRiskIndex);
    }

    private int GetRiskIndex(DawnMoonInfo moonInfo)
    {
        string riskLevel = moonInfo.Level.riskLevel ?? "UNKNOWN";
        riskLevel = riskLevel.StripSpecialCharacters();

        int index = Array.IndexOf(RiskIndexes, riskLevel);
        return index >= 0 ? index : RiskIndexes.Length - 1;
    }
}
#endregion
#region PriceBasedOrdering
public class PriceOrderingStep : IMoonOrderingStep
{
    public IOrderedEnumerable<DawnMoonInfo> ApplyInitial(IEnumerable<DawnMoonInfo> input, bool reverse = false)
    {
        return reverse
            ? input.OrderByDescending(GetPrice)
            : input.OrderBy(GetPrice);
    }

    public IOrderedEnumerable<DawnMoonInfo> ApplyNext(IOrderedEnumerable<DawnMoonInfo> input, bool reverse = false)
    {
        return reverse
            ? input.ThenByDescending(GetPrice)
            : input.ThenBy(GetPrice);
    }

    private int GetPrice(DawnMoonInfo moonInfo)
    {
        return moonInfo.DawnPurchaseInfo.Cost.Provide();
    }
}
#endregion
#region IndexBasedOrdering
public class IndexBasedOrderingStep : IMoonOrderingStep
{
    public IOrderedEnumerable<DawnMoonInfo> ApplyInitial(IEnumerable<DawnMoonInfo> input, bool reverse = false)
    {
        return reverse
            ? input.OrderByDescending(GetIndex)
            : input.OrderBy(GetIndex);
    }

    public IOrderedEnumerable<DawnMoonInfo> ApplyNext(IOrderedEnumerable<DawnMoonInfo> input, bool reverse = false)
    {
        return reverse
            ? input.ThenByDescending(GetIndex)
            : input.ThenBy(GetIndex);
    }

    private int GetIndex(DawnMoonInfo moonInfo)
    {
        return LethalContent.Moons.Values.ToList().IndexOf(moonInfo);
    }
}
#endregion

public class MainGroupAlgorithm
{
    public List<IMoonFilterStep> FilterSteps =
    [
        new VisibleFilterStep(),
    ];

    public List<IMoonOrderingStep> OrderingSteps =
    [
        new RankOrderingStep(),
        new PriceOrderingStep(),
        new IndexBasedOrderingStep(),
    ];

    public IMoonGroupingAlgorithm GroupingAlgorithm = new FixedGroupSizeGroupingAlgorithm();

    public List<MoonGroup> Group(IEnumerable<DawnMoonInfo> input, bool reverse = false)
    {
        IEnumerable<DawnMoonInfo> filteredMoons = input;

        foreach (IMoonFilterStep filterStep in FilterSteps)
        {
            filteredMoons = filterStep.Filter(filteredMoons);
        }

        IOrderedEnumerable<DawnMoonInfo>? orderedMoons = null;

        for (int i = 0; i < OrderingSteps.Count; i++)
        {
            IMoonOrderingStep step = OrderingSteps[i];

            orderedMoons = i == 0
                ? step.ApplyInitial(filteredMoons, reverse)
                : step.ApplyNext(orderedMoons!, reverse);
        }

        IEnumerable<DawnMoonInfo> finalMoons = orderedMoons ?? filteredMoons;

        return GroupingAlgorithm.Group(finalMoons);
    }
}