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
    internal static void Init()
    {
        On.Terminal.Awake += AddShipsToTerminal;
        On.Terminal.TextPostProcess += EditTextPostProcess;
    }

    private static string EditTextPostProcess(On.Terminal.orig_TextPostProcess orig, Terminal self, string modifiedDisplayText, TerminalNode node)
    {
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
        List<BuyableShipPreset> buyableShips = new();
        NamespacedKey currentShip = NamespacedKey.From("lethal_company", "ship");
        ShipSpawnHandler shipSpawnHandler = new();
        shipSpawnHandler.Initialize();

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
                        shipSpawnHandler.ChangeShip(shipDefinition.Key);
                    };

                    queryCommandBuilder.SetQuery(() => $"Are you sure you want to buy {shipDefinition.BuyableShipPreset.ShipName}?"); //add confirm or deny text
                    queryCommandBuilder.SetCancel(() => ""); //change that
                    queryCommandBuilder.SetContinueWord("confirm");
                    queryCommandBuilder.SetCancelWord("deny");
                    queryCommandBuilder.SetQueryEvent(dawnEvent);
                });
            });
        }
        
        ((ITerminal)self).BuyableShips = buyableShips;
        ((ITerminal)self).CurrentShip = shipSpawnHandler.LoadShipFromSave().ToString();

        DuskModContent.Ships.Freeze();
        orig(self);
    }
}

