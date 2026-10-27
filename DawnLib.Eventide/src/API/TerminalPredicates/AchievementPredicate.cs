using Dawn;
using Dusk;
using UnityEngine;

namespace Eventide;

[CreateAssetMenu(menuName = $"{EventideModConstants.TerminalPredicates}/Achievement Unlock Requirement", fileName = "New Achievement Predicate", order = EventideModConstants.PredicateOrder)]
public class AchievementPredicate : DuskTerminalPredicate
{
    [SerializeField]
    string _lockedName = "";

    [SerializeField]
    private TerminalNode _failNode;

    [SerializeReference]
    private EventideAchievementReference _achievement;

    private NamespacedKey _namespacedKey;

    public override void Register(NamespacedKey namespacedKey)
    {
        _namespacedKey = namespacedKey;
        // i dont think anything is needed here.
        // i would've liked to get the definition from the reference here, but this could be loaded before the achievement
    }

    public override TerminalPurchaseResult CanPurchase()
    {
        if (!_achievement.TryResolve(out EventideAchievementDefinition definition))
        {
            DawnPlugin.Logger.LogError($"Failed to resolve the achievement definition for '{_achievement.Key}'. Unlock Requirement NamespacedKey = {_namespacedKey}.");
            return TerminalPurchaseResult.Fail(new TerminalNodeBuilder("AchievementPredicateInternalFail")
                .SetDisplayText("Couldn't find the achievement required, check the logs for more information\n\n")
                .SetClearPreviousText(true)
                .Build());
        }

        if (definition.Completed)
        {
            return TerminalPurchaseResult.Success();
        }

        if (_failNode)
        {
            return TerminalPurchaseResult.Fail(_failNode)
                .SetOverrideName(_lockedName);
        }
        else
        {
            return TerminalPurchaseResult.Hidden();
        }
    }
}