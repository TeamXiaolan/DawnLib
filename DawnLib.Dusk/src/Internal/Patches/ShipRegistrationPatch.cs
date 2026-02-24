using Dawn;
using Dawn.Internal;
using System.Collections.Generic;
using System.Text;

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
        ShipSpawnHandler.Instance.Initialize();

        foreach (DuskShipDefinition shipDefinition in DuskModContent.Ships.Values)
        {
            buyableShips.Add(shipDefinition.BuyableShipPreset);

            if (DuskModContent.Ships.IsFrozen)
                continue;

            //TerminalKeyword keyword = new TerminalKeywordBuilder($"{shipDefinition.BuyableShipPreset.ShipName}BuyKeyword", shipDefinition.BuyableShipPreset.ShipName, ITerminalKeyword.DawnKeywordType.Ships)
            //.SetDefaultVerb(TerminalRefs.BuyKeyword)
            //.Build();

            //LethalContent.TerminalCommands[TerminalCommandKeys.Buy].CommandKeywords.Add(keyword);

            //fancy
            //i THINK i should do mess with it and add to BUY command somehow but head hurts
            TerminalCommandBasicInformation commandBaseInfo = new($"{shipDefinition.BuyableShipPreset.ShipName}Query", "ship category", "test ship api", ClearText.Query);
            DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From($"{shipDefinition.TypedKey.Namespace}", $"{shipDefinition.BuyableShipPreset.ShipName}QueryCommand"), commandBaseInfo, builder =>
            {
                builder.SetKeywords(new List<string>([shipDefinition.BuyableShipPreset.ShipName]));
                builder.DefineSimpleQueryCommand(queryCommandBuilder =>
                {
                    queryCommandBuilder.SetContinueOrCancel(() => $"Are you sure you want to buy {shipDefinition.BuyableShipPreset.ShipName}?"); //add confirm or deny text
                    queryCommandBuilder.SetCancel(() => ""); //change that
                    queryCommandBuilder.SetContinueWord("confirm");
                    queryCommandBuilder.SetCancelWord("deny");
                    queryCommandBuilder.SetQueryEvent((bool value) => ShipSpawnHandler.Instance.ChangeShip(shipDefinition.Key));
                });
            });
        }
        
        ((ITerminal)self).BuyableShips = buyableShips;
        NamespacedKey currentShip = ShipSpawnHandler.Instance.LoadShipFromSave();
        ((ITerminal)self).CurrentShip = currentShip.ToString();

        if (currentShip.IsModded())
            ShipSpawnHandler.Instance.ChangeShip(currentShip);

        DuskModContent.Ships.Freeze();
        orig(self);
    }
}

