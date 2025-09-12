using System;
using System.Collections.Generic;
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
        IL.Terminal.TextPostProcess += HideResults;
        IL.Terminal.TextPostProcess += UseFailedNameResults;
    }

    // this is currently a separate function because this is very specific to vanilla
    private static void HideResults(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel loopStart = null!; // make compiler happy with null!
        Debuggers.Patching?.Log($"transpiling {il.Method.Name} with {nameof(HideResults)}. instructions: {c.Instrs.Count}");

        c.GotoNext(
            i => i.MatchLdloc(7),
            i => i.MatchLdarg(0),
            i => i.MatchLdfld<Terminal>(nameof(Terminal.buyableItemsList)),
            i => i.MatchLdlen(),
            i => i.MatchConvI4(),
            i => i.MatchBlt(out loopStart)
        );
        int targetIndex = c.Index + 2;
        Debuggers.Patching?.Log($"target index = {targetIndex}");

        Debuggers.Patching?.Log($"loopStart = {loopStart}, target = {loopStart.Target}, offset = {loopStart.Target.Offset}");
        c.GotoLabel(loopStart);
        c.Emit(OpCodes.Ldarg_0);
        c.EmitLdfld<Terminal>(nameof(Terminal.buyableItemsList));
        c.EmitLdloc(7);
        c.Emit(OpCodes.Ldelem_Ref);
        c.EmitDelegate((Item item) =>
        {
            Debuggers.Items?.Log($"Checking {item.itemName}");
            DawnItemInfo info = item.GetDawnInfo();
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
        Debuggers.Patching?.Log("did shopitem hidden patch!");

        c.Index = 0;
        c.GotoNext(
            i => i.MatchLdloc(14),
            i => i.MatchLdcI4(1),
            i => i.MatchAdd(),
            i => i.MatchStloc(14)
        );
        c.GotoNext(
            i => i.MatchBlt(out loopStart)
        );
        targetIndex = c.Index + 2;

        c.GotoLabel(loopStart);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitLdfld<Terminal>(nameof(Terminal.ShipDecorSelection));
        c.EmitLdloc(14);
        c.EmitCallvirt<List<TerminalNode>>("get_Item");
        c.EmitDelegate((TerminalNode unlockableNode) =>
        {
            if (unlockableNode.shipUnlockableID < 0 || unlockableNode.shipUnlockableID > StartOfRound.Instance.unlockablesList.unlockables.Count)
            {
                DawnPlugin.Logger.LogWarning($"{unlockableNode.creatureName} ({unlockableNode.name}) has a ship unlockable id of {unlockableNode.shipUnlockableID} which doesn't make sense.");
                return true;
            }
            UnlockableItem unlockableItem = StartOfRound.Instance.unlockablesList.unlockables[unlockableNode.shipUnlockableID];
            DawnUnlockableItemInfo info = unlockableItem.GetDawnInfo();
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
        if (c.TryGotoNext(
            i => i.MatchLdfld<Item>(nameof(Item.itemName))
        ))
        {
            c.Next.OpCode = OpCodes.Nop;

            c.EmitDelegate<Func<Item, string>>((item) =>
            {
                if (!item.HasDawnInfo())
                {
                    DawnPlugin.Logger.LogWarning($"Item: {item.itemName} hasn't been found by DawnLib prior to the terminal being run, please report this!");
                    return item.itemName;
                }
                DawnItemInfo info = item.GetDawnInfo();
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
        }

        c.Index = 0;
        if (c.TryGotoNext(
            i => i.MatchLdfld<UnlockableItem>(nameof(UnlockableItem.unlockableName))
        ))
        {
            c.Next.OpCode = OpCodes.Nop;

            c.EmitDelegate((UnlockableItem unlockable) =>
            {
                if (!unlockable.HasDawnInfo())
                {
                    DawnPlugin.Logger.LogWarning($"Unlockable: {unlockable.unlockableName} hasn't been found by DawnLib prior to the terminal being run, please report this!");
                    return unlockable.unlockableName;
                }
                DawnUnlockableItemInfo info = unlockable.GetDawnInfo();
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
    }
    private static void HandlePredicate(On.Terminal.orig_LoadNewNodeIfAffordable orig, Terminal self, TerminalNode node)
    {
        Debuggers.Patching?.Log($"HandlePredicate: {node}");

        ItemRegistrationHandler.UpdateAllShopItemPrices();

        ITerminalPurchase? purchase = null;
        if (node.buyItemIndex != -1)
        {
            Debuggers.Patching?.Log($"buyItemIndex = {node.buyItemIndex}");
            Item buyingItem = self.buyableItemsList[node.buyItemIndex];
            if (!buyingItem.HasDawnInfo())
            {
                DawnPlugin.Logger.LogWarning($"Item: {buyingItem.itemName} hasn't been found by DawnLib prior to the terminal being run, please report this!");
                orig(self, node);
                return;
            }
            
            DawnItemInfo info = buyingItem.GetDawnInfo();
            DawnShopItemInfo? shopItemInfo = info.ShopInfo;

            if (shopItemInfo != null)
                purchase = shopItemInfo;
        }
        if (node.shipUnlockableID != -1)
        {
            Debuggers.Patching?.Log($"shipUnlockableID = {node.shipUnlockableID}");


            UnlockableItem unlockableItem = StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID];
            if (!unlockableItem.HasDawnInfo())
            {
                DawnPlugin.Logger.LogWarning($"Unlockable: {unlockableItem.unlockableName} hasn't been found by DawnLib prior to the terminal being run, please report this!");
                orig(self, node);
                return;
            }
            DawnUnlockableItemInfo? info = unlockableItem.GetDawnInfo();
            purchase = info;
        }

        // preform predicate
        if (purchase != null)
        {
            Debuggers.Patching?.Log($"has predicate");

            TerminalPurchaseResult result = purchase.PurchasePredicate.CanPurchase();
            if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
            {
                Debuggers.Patching?.Log($"predicate fail");

                orig(self, failedResult.ReasonNode);
                return;
            }
            if (result is TerminalPurchaseResult.HiddenPurchaseResult)
            {
                Debuggers.Patching?.Log($"predicate hidden");

                self.LoadNewNode(new TerminalNodeBuilder("hidden purchase")
                    .SetDisplayText("You're not supposed to be here") // TODO
                    .Build()
                );
                return; // skip orig
            }
        }

        orig(self, node);
    }
}