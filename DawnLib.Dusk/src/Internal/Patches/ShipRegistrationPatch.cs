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

        StringBuilder stringBuilder = new();
        foreach (BuyableShipPreset ship in (List<BuyableShipPreset>)((ITerminal)self).BuyableShips)
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

        List<BuyableShipPreset> buyableShips = new();
        int currentShipId = 0;

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
                    DawnEvent<bool> dawnEvent = new();
                    dawnEvent.OnInvoke += (bool value) =>
                    {
                        ShipSpawnHandler.ChangeShip(currentShipId);
                    };

                    queryCommandBuilder.SetQuery(() => $"Are you sure you want to buy {shipDefinition.BuyableShipPreset.ShipName}?"); //add confirm or deny text
                    queryCommandBuilder.SetCancel(() => ""); //change that
                    queryCommandBuilder.SetContinueWord("confirm");
                    queryCommandBuilder.SetCancelWord("deny");
                    queryCommandBuilder.SetQueryEvent(dawnEvent);
                });
            });

            currentShipId++;
        }
        
        ((ITerminal)self).BuyableShips = buyableShips;
        ((ITerminal)self).CurrentShipId = 0; //TODO: Read save and change ship if needed

        DuskModContent.Ships.Freeze();
        orig(self);
    }
}

