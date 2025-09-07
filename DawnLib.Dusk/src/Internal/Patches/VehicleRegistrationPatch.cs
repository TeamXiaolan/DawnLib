
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Unity.Netcode;
using UnityEngine;

namespace Dusk.Internal;

static class VehicleRegistrationPatch
{
    internal static void Init()
    {
        IL.ItemDropship.DeliverVehicleOnServer += DeliverDuskVehicleOnServer;
        On.Terminal.Awake += RegisterVehicles;
    }

    private static void DeliverDuskVehicleOnServer(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        c.GotoNext(
            i => i.MatchLdarg(0),
            i => i.MatchLdfld<ItemDropship>(nameof(ItemDropship.terminalScript)),
            i => i.MatchLdcI4(-1)
        );
        int targetIndex = c.Index;

        c.Index = 0;
        c.GotoNext(
            i => i.MatchLdarg(0),
            i => i.MatchLdcR4(0.0f),
            i => i.MatchStfld<ItemDropship>(nameof(ItemDropship.shipTimer))
        );

        c.Index += 3;

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((ItemDropship self) =>
        {
            if (self.terminalScript.buyableVehicles[self.terminalScript.orderedVehicleFromTerminal].TryGetDuskDefinition(out DuskVehicleDefinition? vehicleDefinition))
            {
                HandleSpawningDuskVehicle(vehicleDefinition);
                return false;
            }
            return true;
        });
        c.Emit(OpCodes.Brfalse, c.Instrs[targetIndex]);
    }

    private static void HandleSpawningDuskVehicle(DuskVehicleDefinition vehicleDefinition)
    {
        Object.Instantiate(vehicleDefinition.BuyableVehiclePreset.VehiclePrefab, RoundManager.Instance.VehiclesContainer).GetComponent<NetworkObject>().Spawn(false);
        if (vehicleDefinition.BuyableVehiclePreset.SecondaryPrefab != null)
        {
            Object.Instantiate(vehicleDefinition.BuyableVehiclePreset.SecondaryPrefab, RoundManager.Instance.VehiclesContainer).GetComponent<NetworkObject>().Spawn(false);
        }

        if (vehicleDefinition.BuyableVehiclePreset.StationPrefab != null)
        {
            GameObject station = Object.Instantiate(vehicleDefinition.BuyableVehiclePreset.StationPrefab, StartOfRound.Instance.elevatorTransform.position, Quaternion.identity, null);
            station.GetComponent<NetworkObject>().Spawn(false);
            if (station.TryGetComponent(out AutoParentToShip autoParentToShip))
            {
                autoParentToShip.unlockableID = -1;
            }
        }
        else
        {
            // assume it uses magnet somehow?
        }
    }

    private static void RegisterVehicles(On.Terminal.orig_Awake orig, Terminal self)
    {
        TerminalKeyword buyKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
        TerminalKeyword infoKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "info");
        TerminalKeyword confirmPurchaseKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "confirm");
        TerminalKeyword denyPurchaseKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "deny");
        TerminalNode cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;

        List<TerminalKeyword> allKeywordsList = self.terminalNodes.allKeywords.ToList();
        List<CompatibleNoun> allBuyKeywordNounsList = buyKeyword.compatibleNouns.ToList();
        List<CompatibleNoun> allInfoKeywordNounsList = infoKeyword.compatibleNouns.ToList();

        List<BuyableVehicle> buyableVehiclesList = self.buyableVehicles.ToList();
        int currentVehicleIndex = buyableVehiclesList.Count;

        foreach (DuskVehicleDefinition vehicleDefinition in DuskModContent.Vehicles.Values)
        {
            BuyableVehicle buyableVehicle = new()
            {
                vehicleDisplayName = vehicleDefinition.VehicleDisplayName,
                creditsWorth = vehicleDefinition.Config.Cost.Value,
                vehiclePrefab = vehicleDefinition.BuyableVehiclePreset.VehiclePrefab,
                secondaryPrefab = vehicleDefinition.BuyableVehiclePreset.SecondaryPrefab
            };

            buyableVehicle.SetDuskDefinition(vehicleDefinition);
            buyableVehiclesList.Add(buyableVehicle);

            if (DuskModContent.Vehicles.IsFrozen)
                continue;

            TerminalKeyword buyDuskKeyword = new TerminalKeywordBuilder($"{vehicleDefinition.name}BuyKeyword")
                .SetWord(!string.IsNullOrEmpty(vehicleDefinition.BuyableVehiclePreset.BuyKeywordText) ? vehicleDefinition.BuyableVehiclePreset.BuyKeywordText : $"{vehicleDefinition.VehicleDisplayName.ToLowerInvariant()}")
                .SetDefaultVerb(buyKeyword)
                .Build();
            vehicleDefinition.BuyableVehiclePreset.BuyKeyword = buyDuskKeyword;
            allKeywordsList.Add(vehicleDefinition.BuyableVehiclePreset.BuyKeyword);

            TerminalNode confirmDuskNode = new TerminalNodeBuilder($"{vehicleDefinition.name}ConfirmPurchaseNode")
                .SetDisplayText($"Ordered the {vehicleDefinition.VehicleDisplayName}. Your new balance is [playerCredits].\n\nWe are so confident in the quality of this product, it comes with a life-time warranty! If your Cruiser is lost or destroyed, you can get one free replacement. Items cannot be purchased while the vehicle is en route.\n")
                .SetClearPreviousText(true)
                .SetMaxCharactersToType(35)
                .SetBuyVehicleIndex(currentVehicleIndex)
                .SetItemCost(vehicleDefinition.Config.Cost.Value)
                .Build();
            vehicleDefinition.BuyableVehiclePreset.ConfirmPurchaseNode = confirmDuskNode;

            CompatibleNoun[] buyNodeNouns =
            [
                new CompatibleNoun
                {
                    noun = confirmPurchaseKeyword,
                    result = vehicleDefinition.BuyableVehiclePreset.ConfirmPurchaseNode
                },
                new CompatibleNoun
                {
                    noun = denyPurchaseKeyword,
                    result = cancelPurchaseNode
                },
            ];
            TerminalNode buyDuskNode = new TerminalNodeBuilder($"{vehicleDefinition.name}BuyNode")
                .SetDisplayText(!string.IsNullOrEmpty(vehicleDefinition.BuyableVehiclePreset.DisplayNodeText) ? vehicleDefinition.BuyableVehiclePreset.DisplayNodeText : $"You have requested to order the {vehicleDefinition.VehicleDisplayName}.\n[warranty] Total cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\n")
                .SetClearPreviousText(true)
                .SetMaxCharactersToType(15)
                .SetIsConfirmationNode(true)
                .SetBuyVehicleIndex(currentVehicleIndex)
                .SetItemCost(vehicleDefinition.Config.Cost.Value)
                .SetOverrideOptions(true)
                .SetTerminalOptions(buyNodeNouns)
                .Build();
            vehicleDefinition.BuyableVehiclePreset.BuyNode = buyDuskNode;
            CompatibleNoun buyDuskNoun = new()
            {
                noun = buyDuskKeyword,
                result = buyDuskNode
            };
            allBuyKeywordNounsList.Add(buyDuskNoun);

            if (vehicleDefinition.BuyableVehiclePreset.InfoNode != null)
            {
                CompatibleNoun infoCompatibleNoun = new()
                {
                    noun = vehicleDefinition.BuyableVehiclePreset.BuyKeyword,
                    result = vehicleDefinition.BuyableVehiclePreset.InfoNode
                };
                allInfoKeywordNounsList.Add(infoCompatibleNoun);
            }

            currentVehicleIndex++;
        }

        infoKeyword.compatibleNouns = allInfoKeywordNounsList.ToArray();
        buyKeyword.compatibleNouns = allBuyKeywordNounsList.ToArray();
        self.terminalNodes.allKeywords = allKeywordsList.ToArray();
        self.buyableVehicles = buyableVehiclesList.ToArray();

        DuskModContent.Vehicles.Freeze();
        orig(self);
    }
}