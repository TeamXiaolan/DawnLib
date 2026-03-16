using Dawn;
using Dawn.Internal;
using Dawn.Utils;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dusk.Internal;

static class ShipRegistrationPatch
{
    internal static void Init()
    {
        On.Terminal.Awake += AddShipsToTerminal;
        On.Terminal.TextPostProcess += EditTextPostProcess;
        IL.StartOfRound.PositionSuitsOnRack += ActuallyPositionSuitsOnRack;
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

        if (currentShip.IsModded())
            ShipSpawnHandler.Instance.ChangeShip(currentShip);
        else
        {
            PersistentDataContainer? save = DawnLib.GetCurrentSave();
            NamespacedKey currentShipKey = NamespacedKey.From("dawn_lib", "current_ship");
            save?.Set(currentShipKey, currentShip);
        }

        ((ITerminal)self).CurrentShip = currentShip.ToString();

        DuskModContent.Ships.Freeze();
        orig(self);
    }

    private static void ActuallyPositionSuitsOnRack(ILContext il)
    {
        /*
            ...
                component.overrideOffset = true;
		-       component.positionOffset = new Vector3(-2.45f, 2.75f, -8.41f) + rightmostSuitPosition.forward * 0.18f * i;
        +       component.positionOffset = rightmostSuitPosition.position + new Vector3(-1.27f, -0.28f, 7.5f) + rightmostSuitPosition.forward * 0.18f * i;
		-       component.rotationOffset = new Vector3(0f, 90f, 0f);
        +       component.rotationOffset = rightmostSuitPosition.rotation;
		        Debug.Log($"pos: {component.positionOffset}; rot: {component.rotationOffset}");
	        }
	    -    Object.FindObjectsOfType<UnlockableSuit>(includeInactive: true);
        */

        ILCursor c = new ILCursor(il);
        c.GotoNext(
            i => i.MatchLdcR4(-2.45f),
            i => i.MatchLdcR4(2.75f),
            i => i.MatchLdcR4(-8.41f)
        );

        c.RemoveRange(4);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitLdfld<StartOfRound>(nameof(StartOfRound.rightmostSuitPosition));
        c.EmitCallvirt<Transform>("get_position");

        c.Emit(OpCodes.Ldc_R4, -1.27f);
        c.Emit(OpCodes.Ldc_R4, -0.28f);
        c.Emit(OpCodes.Ldc_R4, 7.5f);
        c.Emit(OpCodes.Newobj, AccessTools.Constructor(typeof(Vector3), [typeof(float), typeof(float), typeof(float)]));

        c.GotoNext(i => i.MatchCall<Vector3>("op_Addition"));
        c.Emit(OpCodes.Call, AccessTools.Method(typeof(Vector3), "op_Addition"));

        c.GotoNext(
            i => i.MatchLdcR4(0.0f),
            i => i.MatchLdcR4(90f),
            i => i.MatchLdcR4(0.0f)
        );

        c.RemoveRange(4);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitLdfld<StartOfRound>(nameof(StartOfRound.rightmostSuitPosition));
        //c.EmitCallvirt<Transform>("get_rotation");
        c.EmitCallvirt<Transform>("get_eulerAngles");

        c.GotoNext(
            i => i.MatchPop()
        );

        c.GotoPrev();
        c.GotoPrev();
        c.RemoveRange(3);

        foreach (var instr in il.Instrs)
            Debug.LogWarning($"IL_{instr.Offset:X4}: {instr.OpCode} {instr.Operand}");
    }
}

