using UnityEngine;

namespace Dawn.Dusk;

[CreateAssetMenu(menuName = $"{CRModConstants.TerminalPredicates}/Achievement Unlock Requirement", fileName = "New Achievement Predicate", order = CRModConstants.PredicateOrder)]
public class AchievementPredicate : CRMTerminalPredicate
{
    [SerializeField]
    string _lockedName = "";

    [SerializeField]
    private TerminalNode _failNode;

    [SerializeReference]
    private CRMAchievementReference _achievement;

    private static readonly TerminalNode FailedResolve = new TerminalNodeBuilder("AchievementPredicateInternalFail")
        .SetDisplayText("Couldn't find the achievement required, check the logs for more information\n\n")
        .SetClearPreviousText(true)
        .Build();

    private string _id;
    
    public override void Register(string id)
    {
        _id = id;
        // i dont think anything is needed here.
        // i would've liked to get the definition from the reference here, but this could be loaded before the achievement
    }
    public override TerminalPurchaseResult CanPurchase()
    {
        if (!_achievement.TryResolve(out CRMAchievementDefinition definition))
        {
            DawnPlugin.Logger.LogError($"Failed to resolve the achievement definition for '{_achievement.Key}'. Unlock Requirement id = {_id}.");
            return TerminalPurchaseResult.Fail(FailedResolve);
        }

        if (definition.Completed)
        {
            return TerminalPurchaseResult.Success();
        }
        return TerminalPurchaseResult.Fail(_failNode, _lockedName);
    }
}