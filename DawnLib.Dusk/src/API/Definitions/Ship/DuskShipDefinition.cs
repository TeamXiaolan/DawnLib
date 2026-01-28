using Dawn;
using Dawn.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Ship Definition", menuName = $"{DuskModConstants.Definitions}/Ship Definition")]
public class DuskShipDefinition : DuskContentDefinition<DuskShipDefinition>, INamespaced<DuskShipDefinition>
{
    [field: SerializeField]
    public BuyableShipPreset BuyableShipPreset { get; private set; }

    [field: SerializeField]
    public DuskTerminalPredicate? TerminalPredicate { get; private set; }

    [field: SerializeField]
    public DuskPricingStrategy? PricingStrategy { get; private set; }

    [field: Header("Configs | Misc")]
    [field: SerializeField]
    public bool GenerateDisableUnlockConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateDisablePricingStrategyConfig { get; private set; } = true;

    NamespacedKey<DuskShipDefinition> INamespaced<DuskShipDefinition>.TypedKey => TypedKey.AsTyped<DuskShipDefinition>();

    protected override string EntityNameReference => BuyableShipPreset.ShipName;

    public ShipConfig Config { get; private set; }
    //public DawnVehicleInfo DawnVehicleInfo { get; private set; }

    public override void Register(DuskMod mod)
    {
            if (!BuyableShipPreset.ShipPrefab)
            {
                DuskPlugin.Logger.LogError($"The ship: {BuyableShipPreset.ShipName} has no ship prefab.");
                return;
            }

            if (!BuyableShipPreset.NavmeshPrefab)
            {
                DuskPlugin.Logger.LogError($"The ship: {BuyableShipPreset.ShipName} has no ship navmesh prefab.");
                return;
            }

        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateShipConfig(section);

        BuyableShipPreset.Cost = Config.Cost.Value;
        if (Config.DisableUnlockRequirements?.Value ?? false && TerminalPredicate)
        {
            TerminalPredicate = null;
        }

        if (Config.DisablePricingStrategy?.Value ?? false && PricingStrategy)
        {
            PricingStrategy = null;
        }

        //DawnVehicleInfo = new DawnVehicleInfo(TypedKey, _tags.ToHashSet(), BuyableVehiclePreset.VehiclePrefab, BuyableVehiclePreset.SecondaryPrefab, BuyableVehiclePreset.StationPrefab, new DawnPurchaseInfo(PricingStrategy == null ? new SimpleProvider<int>(Cost) : PricingStrategy, TerminalPredicate ?? ITerminalPurchasePredicate.AlwaysSuccess()), null);

        DuskModContent.Ships.Register(this);
    }

    public override void TryNetworkRegisterAssets()
    {
        if (BuyableShipPreset.ShipPrefab != null && BuyableShipPreset.ShipPrefab.TryGetComponent(out NetworkObject _))
        {
            DawnLib.RegisterNetworkPrefab(BuyableShipPreset.ShipPrefab);
        }
    }

    public ShipConfig CreateShipConfig(ConfigContext context)
    {
        return new ShipConfig
        {
            DisableUnlockRequirements = GenerateDisableUnlockConfig && TerminalPredicate ? context.Bind($"{EntityNameReference} | Disable Unlock Requirements", $"Whether {EntityNameReference} should have it's unlock requirements disabled.", false) : null,
            DisablePricingStrategy = GenerateDisablePricingStrategyConfig && PricingStrategy ? context.Bind($"{EntityNameReference} | Disable Pricing Strategy", $"Whether {EntityNameReference} should have it's pricing strategy disabled.", false) : null,
            Cost = context.Bind($"{EntityNameReference} | Cost", $"Cost for {EntityNameReference} in the shop.", BuyableShipPreset.Cost)
        };
    }
}
