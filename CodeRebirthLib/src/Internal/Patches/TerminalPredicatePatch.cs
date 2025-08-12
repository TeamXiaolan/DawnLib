using System.Linq;

namespace CodeRebirthLib;
static class TerminalPredicatePatch
{
    internal static void Init()
    {
        On.Terminal.LoadNewNodeIfAffordable += HandlePredicate;
    }
    private static void HandlePredicate(On.Terminal.orig_LoadNewNodeIfAffordable orig, Terminal self, TerminalNode node)
    {
        ITerminalPurchasePredicate? predicate = null;
        if (node.buyItemIndex != -1)
        {
            // todo: swap this out for preloader stuff
            CRShopItemInfo? shopItemInfo = LethalContent.Items.Values
                .Where(it => it.ShopInfo != null)
                .Select(it => it.ShopInfo!)
                .FirstOrDefault(it => it.RequestNode.buyItemIndex == node.buyItemIndex);

            if(shopItemInfo != null)
                predicate = shopItemInfo.PurchasePredicate;
        }
        if (node.shipUnlockableID != -1)
        {
            UnlockableItem unlockableItem = StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID];
            CRUnlockableItemInfo? unlockableItemInfo = LethalContent.Unlockables.Values
                .FirstOrDefault(it => it.UnlockableItem == unlockableItem);

            if(unlockableItemInfo != null)
                predicate = unlockableItemInfo.PurchasePredicate;
        }
        
        // preform predicate
        if (predicate != null)
        {
            TerminalPurchaseResult result = predicate.CanPurchase();
            if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
            {
                orig(self, failedResult.ReasonNode);
                return;
            }
        }
        
        orig(self, node);
    }
}