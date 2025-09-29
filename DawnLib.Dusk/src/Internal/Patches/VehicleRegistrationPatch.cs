using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Unity.Netcode;
using UnityEngine;

namespace Dusk.Internal;

static class VehicleRegistrationPatch
{
    internal static void Init()
    {
        On.Terminal.LoadNewNodeIfAffordable += HandlePredicate;
        On.Terminal.LoadNewNodeIfAffordable += UpdatePrices;
        On.ItemDropship.DeliverVehicleOnServer += GetLastDuskVehicleDelivered;
        IL.ItemDropship.DeliverVehicleOnServer += DeliverDuskVehicleOnServer;
        On.Terminal.Awake += RegisterVehicles;
    }

    private static void HandlePredicate(On.Terminal.orig_LoadNewNodeIfAffordable orig, Terminal self, TerminalNode node)
    {
        ITerminalPurchase? purchase = null;
        if (node.buyVehicleIndex != -1)
        {
            Debuggers.Patching?.Log($"buyVehicleIndex = {node.buyVehicleIndex}");
            BuyableVehicle buyingVehicle = self.buyableVehicles[node.buyVehicleIndex];
            if (!buyingVehicle.TryGetDuskDefinition(out DuskVehicleDefinition? duskVehicleDefinition))
            {
                orig(self, node);
                return;
            }
            DawnVehicleInfo info = duskVehicleDefinition.DawnVehicleInfo;
            purchase = info;
        }

        // preform predicate
        if (purchase != null)
        {
            Debuggers.Patching?.Log($"has predicate");

            TerminalPurchaseResult result = purchase.PurchasePredicate.CanPurchase();
            if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
            {
                Debuggers.Patching?.Log($"predicate fail");

                orig(self, failedResult.ReasonNode);
                return;
            }

            if (result is TerminalPurchaseResult.HiddenPurchaseResult)
            {
                Debuggers.Patching?.Log($"predicate hidden");

                self.LoadNewNode(new TerminalNodeBuilder("hidden purchase")
                    .SetDisplayText("You're not supposed to be here") // TODO
                    .Build()
                );

                return; // skip orig
            }
        }

        orig(self, node);
    }

    private static void UpdatePrices(On.Terminal.orig_LoadNewNodeIfAffordable orig, Terminal self, TerminalNode node)
    {
        UpdateAllVehiclePrices();
        orig(self, node);
    }

    internal static void UpdateAllVehiclePrices()
    {
        foreach (DawnVehicleInfo info in DuskModContent.Vehicles.Values.Select(x => x.DawnVehicleInfo))
        {
            if (info.HasTag(DawnLibTags.IsExternal) && !info.HasTag(DawnLibTags.LunarConfig))
                continue;

            UpdateVehiclePrices(info);
        }
    }

    static void UpdateVehiclePrices(DawnVehicleInfo info)
    {
        int cost = info.Cost.Provide();
        info.BuyNode.itemCost = cost;
        info.ConfirmPurchaseNode.itemCost = cost;
    }

    private static void GetLastDuskVehicleDelivered(On.ItemDropship.orig_DeliverVehicleOnServer orig, ItemDropship self)
    {
        TerminalRefs.LastVehicleDelivered = TerminalRefs.Instance.orderedVehicleFromTerminal;
        DuskNetworker.Instance?.SyncVehicleDeliveredServerRpc(TerminalRefs.LastVehicleDelivered);
        orig(self);
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
        Terminal terminal = TerminalRefs.Instance;
        TerminalKeyword buyKeyword = TerminalRefs.BuyKeyword;
        TerminalKeyword infoKeyword = TerminalRefs.InfoKeyword;
        TerminalKeyword confirmPurchaseKeyword = TerminalRefs.ConfirmPurchaseKeyword;
        TerminalKeyword denyPurchaseKeyword = TerminalRefs.DenyKeyword;
        TerminalNode cancelPurchaseNode = TerminalRefs.CancelPurchaseNode;

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
            vehicleDefinition.DawnVehicleInfo.BuyKeyword = buyDuskKeyword;
            allKeywordsList.Add(vehicleDefinition.DawnVehicleInfo.BuyKeyword);

            TerminalNode confirmDuskNode = new TerminalNodeBuilder($"{vehicleDefinition.name}ConfirmPurchaseNode")
                .SetDisplayText($"Ordered the {vehicleDefinition.VehicleDisplayName}. Your new balance is [playerCredits].\n\nWe are so confident in the quality of this product, it comes with a life-time warranty! If your {vehicleDefinition.VehicleDisplayName} is lost or destroyed, you can get one free replacement. Items cannot be purchased while the vehicle is en route.\n")
                .SetClearPreviousText(true)
                .SetMaxCharactersToType(35)
                .SetBuyVehicleIndex(currentVehicleIndex)
                .SetItemCost(vehicleDefinition.Config.Cost.Value)
                .Build();
            vehicleDefinition.DawnVehicleInfo.ConfirmPurchaseNode = confirmDuskNode;

            CompatibleNoun[] buyNodeNouns =
            [
                new CompatibleNoun
                {
                    noun = confirmPurchaseKeyword,
                    result = vehicleDefinition.DawnVehicleInfo.ConfirmPurchaseNode
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
            vehicleDefinition.DawnVehicleInfo.BuyNode = buyDuskNode;
            CompatibleNoun buyDuskNoun = new()
            {
                noun = buyDuskKeyword,
                result = buyDuskNode
            };
            allBuyKeywordNounsList.Add(buyDuskNoun);

            if (vehicleDefinition.DawnVehicleInfo.InfoNode != null)
            {
                CompatibleNoun infoCompatibleNoun = new()
                {
                    noun = vehicleDefinition.DawnVehicleInfo.BuyKeyword,
                    result = vehicleDefinition.DawnVehicleInfo.InfoNode
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