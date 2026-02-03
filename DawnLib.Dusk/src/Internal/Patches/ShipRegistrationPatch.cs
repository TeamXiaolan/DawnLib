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
        foreach (BuyableShipPreset ship in (List<BuyableShipPreset>)((ITerminalBuyableShips)self).buyableShips)
            stringBuilder.Append("\n* " + ship.ShipName + "  //  Price: $" + ship.Cost);

        modifiedDisplayText = modifiedDisplayText.Replace("[buyableShipsList]", stringBuilder.ToString());

        return modifiedDisplayText;
    }

    private static void AddShipsToTerminal(On.Terminal.orig_Awake orig, Terminal self)
    {
        //TerminalKeyword buyKeyword = TerminalRefs.BuyKeyword;
        //TerminalKeyword infoKeyword = TerminalRefs.InfoKeyword;
        //TerminalKeyword confirmPurchaseKeyword = TerminalRefs.ConfirmPurchaseKeyword;
        //TerminalKeyword denyPurchaseKeyword = TerminalRefs.DenyKeyword;
        //TerminalNode cancelPurchaseNode = TerminalRefs.CancelPurchaseNode;

        //List<TerminalKeyword> allKeywordsList = self.terminalNodes.allKeywords.ToList();
        //List<CompatibleNoun> allBuyKeywordNounsList = buyKeyword.compatibleNouns.ToList();
        //List<CompatibleNoun> allInfoKeywordNounsList = infoKeyword.compatibleNouns.ToList();

        List <BuyableShipPreset> buyableShips = new List<BuyableShipPreset>();
        int currentShipIndex = 0;

        foreach (DuskShipDefinition shipDefinition in DuskModContent.Ships.Values)
        {
            buyableShips.Add(shipDefinition.BuyableShipPreset);

            if (DuskModContent.Ships.IsFrozen)
                continue;

            //fancy
            DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From($"{shipDefinition.TypedKey.Namespace}", $"{shipDefinition.BuyableShipPreset.ShipName}QueryCommand"), $"{shipDefinition.BuyableShipPreset.ShipName}Query", builder =>
            {
                builder.SetEnabled(new SimpleProvider<bool>(true));
                builder.SetMainText(() => "confirmed\n\n"); //change that
                builder.SetKeywords(new SimpleProvider<List<string>>([shipDefinition.BuyableShipPreset.ShipName]));
                builder.SetCategoryName("ShipShopCategory");
                builder.SetClearTextFlags(TerminalCommandRegistration.ClearText.Query);
                builder.SetDescription($"Buy {shipDefinition.BuyableShipPreset.ShipName}.");
                builder.DefineQueryCommand(queryCommandBuilder =>
                {
                    queryCommandBuilder.SetQuery(() => $"Are you sure you want to buy {shipDefinition.BuyableShipPreset.ShipName}?"); //add confirm or deny text
                    queryCommandBuilder.SetCancel(() => ""); //change that
                    queryCommandBuilder.SetContinueWord("confirm");
                    queryCommandBuilder.SetCancelWord("deny");
                    //queryCommandBuilder.SetContinueEvent or smth like that
                });
            });

            currentShipIndex++;
        }
        
        ((ITerminalBuyableShips)self).buyableShips = buyableShips;

        DuskModContent.Ships.Freeze();
        orig(self);
    }


    //TODO: this patch prob should be more abstract so we could use terminal events for something like terminal event registration in editor or smth like that
    private static void AddEventToTerminal(On.Terminal.orig_RunTerminalEvents orig, Terminal self, TerminalNode node)
    {
        orig(self, node);
    }


}

