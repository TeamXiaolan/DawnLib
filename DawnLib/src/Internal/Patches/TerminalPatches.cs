using System;
using System.Collections.Generic;
using Dawn.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Dawn.Internal;
static class TerminalPatches
{
    internal static event Action OnTerminalAwake = delegate { };
    internal static event Action OnTerminalDisable = delegate { };
    internal static void Init()
    {
        On.Terminal.Awake += TerminalAwakeHook;
        On.Terminal.Start += TerminalStartHook;
        On.Terminal.OnDisable += TerminalDisableHook;
        On.Terminal.CheckForExactSentences += CheckForExactSentencesPrefix;
        On.Terminal.ParseWord += ParseWordPrefix;
        On.Terminal.ParsePlayerSentence += HandleDawnCommand;
        On.Terminal.LoadNewNodeIfAffordable += HandlePredicate;
        On.Terminal.TextPostProcess += UpdateItemPrices;
        IL.Terminal.TextPostProcess += HideResults;
        IL.Terminal.TextPostProcess += UseFailedNameResults;
    }


    private static TerminalKeyword CheckForExactSentencesPrefix(On.Terminal.orig_CheckForExactSentences orig, Terminal self, string playerWord)
    {
        //reset last command values to be empty/null
        //this runs before ParseWordPrefix
        self.SetLastCommand(string.Empty);
        self.SetLastVerb(null!);
        self.SetLastNoun(null!);

        if (self.DawnTryResolveKeyword(playerWord, out TerminalKeyword NonNullResult))
        {
            self.UpdateLastKeywordParsed(NonNullResult);
            self.SetLastCommand(playerWord.GetExactMatch(NonNullResult.word));
            return NonNullResult;
        }
        else
        {
            //run original
            TerminalKeyword vanillaResult = orig(self, playerWord);
            self.UpdateLastKeywordParsed(vanillaResult);
            self.SetLastCommand(playerWord);
            return vanillaResult;
        }

    }

    private static TerminalKeyword ParseWordPrefix(On.Terminal.orig_ParseWord orig, Terminal self, string playerWord, int specificityRequired)
    {
        if (self.DawnTryResolveKeyword(playerWord, out TerminalKeyword NonNullResult))
        {
            self.UpdateLastKeywordParsed(NonNullResult);
            self.SetLastCommand(playerWord.GetExactMatch(NonNullResult.word));
            return NonNullResult;
        }
        else
        {
            //run original
            TerminalKeyword vanillaResult = orig(self, playerWord, specificityRequired);
            self.UpdateLastKeywordParsed(vanillaResult);
            return vanillaResult;
        }

    }

    private static void TerminalDisableHook(On.Terminal.orig_OnDisable orig, Terminal self)
    {
        //All commands use this event to destroy themselves between lobby loads
        OnTerminalDisable.Invoke();

        //still need to run the method
        orig(self);
    }

    private static void TerminalStartHook(On.Terminal.orig_Start orig, Terminal self)
    {
        orig(self);

        //assign priorities to any remaining keywords that have not received a value yet
        //also assign descriptions/category if unassigned
        //doing this in start to give time after Terminal.Awake where commands are created
        foreach (TerminalKeyword keyword in self.terminalNodes.allKeywords)
        {
            keyword.TryAssignType();
            if (string.IsNullOrEmpty(keyword.GetKeywordCategory()))
            {
                keyword.SetKeywordCategory(keyword.GetKeywordPriority().ToString());
            }

            if (string.IsNullOrEmpty(keyword.GetKeywordDescription()))
            {
                if (keyword.TryGetKeywordInfoText(out string result))
                {
                    keyword.SetKeywordDescription(result.Trim());
                }
                else
                {
                    keyword.SetKeywordDescription($"No information on the terminal keyword [ {keyword.word} ]");
                }
            }
        }
    }

    private static void TerminalAwakeHook(On.Terminal.orig_Awake orig, Terminal self)
    {
        orig(self);
        //below will have many terminal commands begin building on the below invoke
        //only commands with a custom defined build event will not use this event
        OnTerminalAwake.Invoke();
    }

    private static TerminalNode HandleDawnCommand(On.Terminal.orig_ParsePlayerSentence orig, Terminal self)
    {
        //Get vanilla result
        TerminalNode terminalNode = orig(self);

        //updates the node's displaytext based on it's NodeFunction Func<string> that was injected (if not null)
        if (terminalNode.HasCommandFunction())
        {
            terminalNode.displayText = terminalNode.GetCommandFunction().Invoke();
        }

        return terminalNode;
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

            TerminalPurchaseResult result = shopInfo.DawnPurchaseInfo.PurchasePredicate.CanPurchase();

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
            DawnUnlockableItemInfo? info = unlockableItem.GetDawnInfo();
            if (info == null)
            {
                DawnPlugin.Logger.LogWarning($"{unlockableNode.creatureName} ({unlockableNode.name}) of {unlockableItem.unlockableName} has no dawn info.");
                return true;
            }
            TerminalPurchaseResult result = info.DawnPurchaseInfo.PurchasePredicate.CanPurchase();
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
        MoonRegistrationHandler.UpdateAllPrices();
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

                TerminalPurchaseResult result = shopInfo.DawnPurchaseInfo.PurchasePredicate.CanPurchase();
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
                TerminalPurchaseResult result = info.DawnPurchaseInfo.PurchasePredicate.CanPurchase();
                if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
                {
                    if (!string.IsNullOrEmpty(failedResult.OverrideName))
                    {
                        Debuggers.Unlockables?.Log($"Overriding name of {info.Key} with {failedResult.OverrideName}");
                        return failedResult.OverrideName;
                    }
                }

                return unlockable.unlockableName;
            });
        }
    }
    private static void HandlePredicate(On.Terminal.orig_LoadNewNodeIfAffordable orig, Terminal self, TerminalNode node)
    {
        Debuggers.Patching?.Log($"HandlePredicate: {node}");

        ItemRegistrationHandler.UpdateAllShopItemPrices();
        UnlockableRegistrationHandler.UpdateAllUnlockablePrices();

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
            {
                purchase = shopItemInfo.DawnPurchaseInfo;
            }
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
            DawnUnlockableItemInfo info = unlockableItem.GetDawnInfo();
            purchase = info.DawnPurchaseInfo;
        }

        if (node.buyRerouteToMoon >= 0)
        {
            Debuggers.Patching?.Log($"buyRerouteToMoon = {node.buyRerouteToMoon}");
            purchase = StartOfRound.Instance.levels[node.buyRerouteToMoon].GetDawnInfo().DawnPurchaseInfo;
        }

        // preform predicate
        if (purchase != null)
        {
            Debuggers.Patching?.Log($"has predicate");

            TerminalPurchaseResult result = purchase.PurchasePredicate.CanPurchase();
            if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
            {
                Debuggers.Patching?.Log($"predicate fail");

                self.LoadNewNode(failedResult.ReasonNode);
                return;
            }

            if (result is TerminalPurchaseResult.HiddenPurchaseResult hiddenResult && hiddenResult.IsFailure)
            {
                Debuggers.Patching?.Log($"predicate hidden failure");

                self.LoadNewNode(hiddenResult.ReasonNode);
                return; // skip orig
            }
        }

        orig(self, node);
    }
}