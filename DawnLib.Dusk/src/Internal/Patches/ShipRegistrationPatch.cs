using Dawn;
using Dawn.Interfaces;
using Dawn.Internal;
using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace Dusk.Internal;

static class ShipRegistrationPatch
{
    //TODO: show buyable ships in store rotataion if its enabled in config
    internal static void Init()
    {
        On.Terminal.Awake += AddShipsToTerminal;
        On.Terminal.RunTerminalEvents += AddEventToTerminal;
        On.Terminal.TextPostProcess += EditTextPostProcess;
    }

    //TODO: rewrite shop entirely?
    private static string EditTextPostProcess(On.Terminal.orig_TextPostProcess orig, Terminal self, string modifiedDisplayText, TerminalNode node)
    {
        /*if (!node.displayText.Contains("[buyableShipsList]"))
        {
            node.displayText = node.displayText.Replace("[buyableVehiclesList]", "[buyableVehiclesList]\n[buyableShipsList]");
        }*/

        if (!modifiedDisplayText.Contains("[buyableShipsList]"))
        {
            modifiedDisplayText = modifiedDisplayText.Replace("[buyableVehiclesList]", "[buyableVehiclesList]\n[buyableShipsList]");
        }

        modifiedDisplayText = orig(self,modifiedDisplayText, node);

        StringBuilder stringBuilder = new StringBuilder();
        foreach (BuyableShip ship in (List<BuyableShip>)((ITerminalBuyableShips)self).buyableShips)
            stringBuilder.Append("\n* " + ship.ShipName + "  //  Price: $" + ship.Cost);

        modifiedDisplayText = modifiedDisplayText.Replace("[buyableShipsList]", stringBuilder.ToString());

        return modifiedDisplayText;
    }

    private static void AddShipsToTerminal(On.Terminal.orig_Awake orig, Terminal self)
    {
        TerminalKeyword buyKeyword = TerminalRefs.BuyKeyword;
        TerminalKeyword infoKeyword = TerminalRefs.InfoKeyword;
        TerminalKeyword confirmPurchaseKeyword = TerminalRefs.ConfirmPurchaseKeyword;
        TerminalKeyword denyPurchaseKeyword = TerminalRefs.DenyKeyword;
        TerminalNode cancelPurchaseNode = TerminalRefs.CancelPurchaseNode;

        List<TerminalKeyword> allKeywordsList = self.terminalNodes.allKeywords.ToList();
        List<CompatibleNoun> allBuyKeywordNounsList = buyKeyword.compatibleNouns.ToList();
        List<CompatibleNoun> allInfoKeywordNounsList = infoKeyword.compatibleNouns.ToList();

        //List<BuyableShip> buyableShips = (List<BuyableShip>)((ITerminalBuyableShips)self).buyableShips; //wtf
        //i forgot that this list is not vanila and never initialised
        List<BuyableShip> buyableShips = new List<BuyableShip>();
        int currentShipIndex = 0;

        foreach (DuskShipDefinition shipDefinition in DuskModContent.Ships.Values)
        {
            buyableShips.Add(shipDefinition.Ship);

            if (DuskModContent.Ships.IsFrozen)
                continue;

            TerminalKeyword buyDuskShipKeyword = new TerminalKeywordBuilder($"{shipDefinition.Ship.ShipName}BuyKeyword")
                .SetWord(shipDefinition.Ship.ShipName.ToLowerInvariant())
                .SetDefaultVerb(buyKeyword)
                .Build();

            allKeywordsList.Add(buyDuskShipKeyword);

            TerminalNode confirmDuskNode = new TerminalNodeBuilder($"{shipDefinition.Ship.ShipName}ConfirmPurchaseNode")
                .SetDisplayText($"Ordered the {shipDefinition.Ship.ShipName}. Your new balance is [playerCredits].\n\nWe are so confident in the quality of this product, it comes with a life-time warranty! If your {shipDefinition.Ship.ShipName} is lost or destroyed, you can get one free replacement.\n")
                .SetClearPreviousText(true)
                .SetMaxCharactersToType(35)
                .SetItemCost(shipDefinition.Ship.Cost)
                .SetIsConfirmationNode(true)
                .Build();

            CompatibleNoun[] buyNodeNouns =
            [
                new CompatibleNoun
                    {
                        noun = confirmPurchaseKeyword,
                        result = confirmDuskNode
                    },
                    new CompatibleNoun
                    {
                        noun = denyPurchaseKeyword,
                        result = cancelPurchaseNode
                    },
                ];

            TerminalNode buyDuskNode = new TerminalNodeBuilder($"{shipDefinition.Ship.ShipName}BuyNode")
                .SetDisplayText($"You have requested to order the {shipDefinition.Ship.ShipName}\n")
                .SetClearPreviousText(true)
                .SetMaxCharactersToType(15)
                .SetItemCost(shipDefinition.Ship.Cost)
                .SetOverrideOptions(true)
                .SetTerminalOptions(buyNodeNouns)
                .SetTerminalEvent("shipPurchase")
                .Build();
            buyDuskNode.SetBuyShipIndex(currentShipIndex);

            CompatibleNoun buyDuskNoun = new()
            {
                noun = buyDuskShipKeyword,
                result = buyDuskNode
            };
            allBuyKeywordNounsList.Add(buyDuskNoun);
            currentShipIndex++;
        }


        infoKeyword.compatibleNouns = allInfoKeywordNounsList.ToArray();
        buyKeyword.compatibleNouns = allBuyKeywordNounsList.ToArray();
        self.terminalNodes.allKeywords = allKeywordsList.ToArray();
        ((ITerminalBuyableShips)self).buyableShips = buyableShips;

        DuskModContent.Ships.Freeze();
        orig(self);
    }


    //TODO: this patch prob should be more abstract so we could use terminal events for something like terminal event registration in editor or smth like that
    private static void AddEventToTerminal(On.Terminal.orig_RunTerminalEvents orig, Terminal self, TerminalNode node)
    {
        orig(self, node);

        if (node.terminalEvent == "shipPurchase" && StartOfRound.Instance.inShipPhase && !StartOfRound.Instance.firingPlayersCutsceneRunning)
        {
            //for some reason i cant figure out why game doesnt want to spend money when i buy ship
            //so if you can figure it out - please fix it and erase this self.groupCredits = wawawa
            //
            //int cost = ((List<BuyableShip>)((ITerminalBuyableShips)self).buyableShips)[(int)((ITerminalNodeShipIndex)node).buyShipIndex].Cost; //god
            self.groupCredits = Mathf.Clamp(self.groupCredits - node.itemCost, 0, 10000000);
            //spawn ship here
        }
    }


}

