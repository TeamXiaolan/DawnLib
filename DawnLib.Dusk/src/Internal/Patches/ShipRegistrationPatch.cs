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
        if (!node.displayText.Contains("[buyableVehiclesList]"))
            return orig(self, modifiedDisplayText, node);
        else if (!node.displayText.Contains("[buyableShipsList]"))
            node.displayText.Replace("[buyableVehiclesList]", "[buyableVehiclesList]\n[buyableShipsList]");

        if (!modifiedDisplayText.Contains("[buyableShipsList]"))
            modifiedDisplayText = modifiedDisplayText.Replace("[buyableVehiclesList]", "[buyableVehiclesList]\n[buyableShipsList]");

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
        }
        
        ((ITerminal)self).BuyableShips = buyableShips;
        NamespacedKey currentShip = ShipSpawnHandler.Instance.LoadShipFromSave();
        ((ITerminal)self).CurrentShip = currentShip.ToString();

        if (currentShip.IsModded())
            ShipSpawnHandler.Instance.ChangeShip(currentShip);
        else
        {
            PersistentDataContainer? save = DawnLib.GetCurrentSave();
            NamespacedKey currentShipKey = NamespacedKey.From("dawn_lib", "current_ship");
            save?.Set(currentShipKey, currentShip);
        }

        DuskModContent.Ships.Freeze();
        orig(self);
    }
}

