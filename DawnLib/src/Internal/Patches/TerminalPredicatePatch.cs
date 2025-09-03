using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Dawn.Internal;
static class TerminalPredicatePatch
{
    internal static void Init()
    {
        On.Terminal.LoadNewNodeIfAffordable += HandlePredicate;
        IL.Terminal.TextPostProcess += UseFailedResultName;
    }
    internal static void UseFailedResultName(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        CodeRebirthLibPlugin.Logger.LogDebug($"transpiling {il.Method.Name} with UseFailedResultName. instructions: {c.Instrs.Count}");
        c.GotoNext(
            i => i.MatchLdfld<Item>(nameof(Item.itemName))
        );
        
        c.Next.OpCode = OpCodes.Nop;

        c.EmitDelegate<Func<Item, string>>((item) =>
        {
            if (!item.TryGetCRInfo(out CRItemInfo? info))
                return item.itemName;
            
            CRShopItemInfo? shopInfo = info.ShopInfo;
            if (shopInfo == null)
                return item.itemName;
            
            TerminalPurchaseResult result = shopInfo.PurchasePredicate.CanPurchase();
            if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
            {
                return failedResult.OverrideName ?? item.itemName;
            }
            
            return item.itemName;
        });

        c.Index = 0;
        c.GotoNext(
            i => i.MatchLdfld<UnlockableItem>(nameof(UnlockableItem.unlockableName))
        );
        c.Next.OpCode = OpCodes.Nop;
        
        c.EmitDelegate((UnlockableItem unlockable) =>
        {
            if (!unlockable.TryGetCRInfo(out CRUnlockableItemInfo? info))
                return unlockable.unlockableName;

            TerminalPurchaseResult result = info.PurchasePredicate.CanPurchase();
            if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
            {
                return failedResult.OverrideName;
            }
            
            return unlockable.unlockableName;
        });
    }
    private static void HandlePredicate(On.Terminal.orig_LoadNewNodeIfAffordable orig, Terminal self, TerminalNode node)
    {
        ITerminalPurchase? purchase = null;
        if (node.buyItemIndex != -1)
        {
            Item buyingItem = self.buyableItemsList[node.buyItemIndex];
            if (!buyingItem.TryGetCRInfo(out CRItemInfo? info))
            {
                CodeRebirthLibPlugin.Logger.LogWarning($"Couldn't get CR info for {buyingItem.itemName}");
                return;
            }

            CRShopItemInfo? shopItemInfo = info.ShopInfo;

            if (shopItemInfo != null)
                purchase = shopItemInfo;
        }
        if (node.shipUnlockableID != -1)
        {
            UnlockableItem unlockableItem = StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID];
            if (!unlockableItem.TryGetCRInfo(out CRUnlockableItemInfo? info))
            {
                CodeRebirthLibPlugin.Logger.LogWarning($"Couldn't get CR info for {unlockableItem.unlockableName}");
                return;
            }
            
            purchase = info;
        }

        // preform predicate
        if (purchase != null)
        {
            TerminalPurchaseResult result = purchase.PurchasePredicate.CanPurchase();
            if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
            {
                orig(self, failedResult.ReasonNode);
                return;
            }
        }

        orig(self, node);
    }
}