using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

namespace CodeRebirthLib;
static class ItemRegistrationHandler
{
    internal static void Init()
    {
        On.Terminal.Awake += RegisterShopItems;
    }
    private static void RegisterShopItems(On.Terminal.orig_Awake orig, Terminal self)
    {
        TerminalKeyword buyKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
        TerminalKeyword infoKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "info");
        TerminalKeyword confirmPurchaseKeyword = self.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "confirm");
        TerminalKeyword denyPurchaseKeyword = self.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "deny");
        TerminalNode cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
        
        // first add modded content
        List<Item> newBuyableList = self.buyableItemsList.ToList();
        List<CompatibleNoun> newBuyCompatibleNouns = buyKeyword.compatibleNouns.ToList();
        List<CompatibleNoun> newInfoCompatibleNouns = infoKeyword.compatibleNouns.ToList();
        List<TerminalKeyword> newTerminalKeywords = self.terminalNodes.allKeywords.ToList();
        foreach (CRItemInfo itemInfo in LethalContent.Items.Values)
        {
            CRShopItemInfo? shopInfo = itemInfo.ShopInfo;
            if(shopInfo == null && itemInfo.Key.IsModded()) continue; // also ensure not to register vanilla stuff again
            string simplifiedItemName = itemInfo.Item.itemName.Replace(" ", "-");
            
            newBuyableList.Add(itemInfo.Item);
            TerminalNode receiptNode = shopInfo!.ReceiptNode;
            TerminalNode requestNode = shopInfo.RequestNode;
            
            receiptNode.buyItemIndex = newBuyableList.Count - 1;
            receiptNode.itemCost = shopInfo.Cost;
            
            requestNode.buyItemIndex = newBuyableList.Count - 1;
            requestNode.isConfirmationNode = true;
            requestNode.overrideOptions = true;
            requestNode.itemCost = shopInfo.Cost;
            requestNode.terminalOptions =
            [
                new CompatibleNoun()
                {
                    noun = confirmPurchaseKeyword,
                    result = receiptNode
                },
                new CompatibleNoun()
                {
                    noun = denyPurchaseKeyword,
                    result = cancelPurchaseNode
                }
            ];
            
            TerminalKeyword buyItemKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            buyItemKeyword.name = simplifiedItemName.ToLowerInvariant();
            buyItemKeyword.word = simplifiedItemName.ToLowerInvariant();
            buyItemKeyword.isVerb = false;
            buyItemKeyword.compatibleNouns = null;
            buyItemKeyword.specialKeywordResult = null;
            buyItemKeyword.defaultVerb = buyKeyword;
            buyItemKeyword.accessTerminalObjects = false;
            newTerminalKeywords.Add(buyItemKeyword);
            
            newBuyCompatibleNouns.Add(new CompatibleNoun()
            {
                noun = buyItemKeyword,
                result = requestNode
            });
            newInfoCompatibleNouns.Add(new CompatibleNoun()
            {
                noun = buyItemKeyword,
                result = shopInfo.InfoNode
            });
        }
        
        // then, before freezing registry, add vanilla content
        if (!LethalContent.Items.IsFrozen) // effectively check for a lobby reload
        {
            foreach (TerminalNode vanillaItemResultNode in buyKeyword.compatibleNouns.Select(it => it.result))
            {
                // todo
            }
        }

        // update terminal references to include new stuff
        self.buyableItemsList = newBuyableList.ToArray();
        infoKeyword.compatibleNouns = newInfoCompatibleNouns.ToArray();
        buyKeyword.compatibleNouns = newBuyCompatibleNouns.ToArray();
        self.terminalNodes.allKeywords = newTerminalKeywords.ToArray();
        
        LethalContent.Items.Freeze();
        orig(self);
    }
}