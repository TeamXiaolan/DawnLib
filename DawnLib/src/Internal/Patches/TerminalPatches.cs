using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dawn.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Dawn.Internal;
static class TerminalPatches
{
    internal static void Init()
    {
        On.Terminal.LoadNewNodeIfAffordable += HandlePredicate;
        On.Terminal.TextPostProcess += UpdateItemPrices;
        IL.Terminal.TextPostProcess += UseFailedNameResults;
        IL.Terminal.TextPostProcess += HideResults;
    }
    
    // this is currently a separate function because this is very specific to vanilla
    private static void HideResults(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel loopStart = null!; // make compiler happy with null!
        c.GotoNext(
            i => i.MatchLdloc(7),
            i => i.MatchLdarg(0),
            i => i.MatchLdfld<Terminal>(nameof(Terminal.buyableItemsList)),
            i => i.MatchLdlen(),
            i => i.MatchConvI4(),
            i => i.MatchBlt(out loopStart)
        );
        int targetIndex = c.Index - 4;

        c.Index = loopStart.Target.Offset;
        c.EmitLdfld<Terminal>(nameof(Terminal.buyableItemsList));
        c.EmitLdloc(7);
        c.Emit(OpCodes.Ldelem_Ref);
        c.EmitDelegate<Func<Item, bool>>((Item item) =>
        {
            if (!item.TryGetDawnInfo(out DawnItemInfo? info))
                return true;

            DawnShopItemInfo? shopInfo = info.ShopInfo;
            if (shopInfo == null)
                return true;

            TerminalPurchaseResult result = shopInfo.PurchasePredicate.CanPurchase();

            if (result is TerminalPurchaseResult.HiddenPurchaseResult)
            {
                Debuggers.Items?.Log($"Hiding {info.Key}");
                return false;
            }
            return true;
        });
        c.Emit(OpCodes.Brfalse, c.Instrs[targetIndex]);

        c.Index = 0;
        c.GotoNext(
            i => i.MatchLdloc(14),
            i => i.MatchLdarg(0),
            i => i.MatchLdfld<Terminal>(nameof(Terminal.ShipDecorSelection)),
            i => i.MatchCallvirt<List<TerminalNode>>(nameof(List<TerminalNode>.Count)),
            i => i.MatchBlt(out loopStart)
        );
        targetIndex = c.Index - 4;
        
        c.EmitLdfld<Terminal>(nameof(Terminal.ShipDecorSelection));
        c.EmitLdloc(14);
        c.EmitCallvirt<List<TerminalNode>>("get_Item");
        c.EmitDelegate((TerminalNode unlockableNode) =>
        {
            UnlockableItem unlockableItem = StartOfRound.Instance.unlockablesList.unlockables[unlockableNode.shipUnlockableID];
            if (!unlockableItem.TryGetDawnInfo(out DawnUnlockableItemInfo? info))
                return true;

            TerminalPurchaseResult result = info.PurchasePredicate.CanPurchase();
            if (result is TerminalPurchaseResult.HiddenPurchaseResult)
            {
                Debuggers.Unlockables?.Log($"Hiding {info.Key}");
                return false;
            }
            return true;
        });
        c.Emit(OpCodes.Brfalse, c.Instrs[targetIndex]);
    }
    
    
    private static string UpdateItemPrices(On.Terminal.orig_TextPostProcess orig, Terminal self, string modifieddisplaytext, TerminalNode node)
    {
        ItemRegistrationHandler.UpdateAllShopItemPrices();
        return orig(self, modifieddisplaytext, node);
    }
    
    internal static void UseFailedNameResults(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        DawnPlugin.Logger.LogDebug($"transpiling {il.Method.Name} with {nameof(UseFailedNameResults)}. instructions: {c.Instrs.Count}");
        c.GotoNext(
            i => i.MatchLdfld<Item>(nameof(Item.itemName))
        );
        c.Next.OpCode = OpCodes.Nop;

        c.EmitDelegate<Func<Item, string>>((item) =>
        {
            if (!item.TryGetDawnInfo(out DawnItemInfo? info))
                return item.itemName;

            DawnShopItemInfo? shopInfo = info.ShopInfo;
            if (shopInfo == null)
                return item.itemName;

            TerminalPurchaseResult result = shopInfo.PurchasePredicate.CanPurchase();
            if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
            {
                if (failedResult.OverrideName != null)
                {
                    Debuggers.Items?.Log($"Overriding name of {info.Key} with {failedResult.OverrideName}");
                }
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
            if (!unlockable.TryGetDawnInfo(out DawnUnlockableItemInfo? info))
                return unlockable.unlockableName;

            TerminalPurchaseResult result = info.PurchasePredicate.CanPurchase();
            if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
            {
                if (failedResult.OverrideName != null)
                {
                    Debuggers.Unlockables?.Log($"Overriding name of {info.Key} with {failedResult.OverrideName}");
                }
                return failedResult.OverrideName;
            }

            return unlockable.unlockableName;
        });
    }
    private static void HandlePredicate(On.Terminal.orig_LoadNewNodeIfAffordable orig, Terminal self, TerminalNode node)
    {
        ItemRegistrationHandler.UpdateAllShopItemPrices();
        
        ITerminalPurchase? purchase = null;
        if (node.buyItemIndex != -1)
        {
            Item buyingItem = self.buyableItemsList[node.buyItemIndex];
            if (!buyingItem.TryGetDawnInfo(out DawnItemInfo? info))
            {
                DawnPlugin.Logger.LogWarning($"Couldn't get CR info for {buyingItem.itemName}");
                return;
            }

            DawnShopItemInfo? shopItemInfo = info.ShopInfo;

            if (shopItemInfo != null)
                purchase = shopItemInfo;
        }
        if (node.shipUnlockableID != -1)
        {
            UnlockableItem unlockableItem = StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID];
            if (!unlockableItem.TryGetDawnInfo(out DawnUnlockableItemInfo? info))
            {
                DawnPlugin.Logger.LogWarning($"Couldn't get CR info for {unlockableItem.unlockableName}");
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
            if (result is TerminalPurchaseResult.HiddenPurchaseResult)
            {
                orig(self,
                    new TerminalNodeBuilder("hidden purchase")
                        .SetDisplayText("You're not supposed to be here") // TODO
                        .Build()
                ); 
                return;
            }
        }

        orig(self, node);
    }
}