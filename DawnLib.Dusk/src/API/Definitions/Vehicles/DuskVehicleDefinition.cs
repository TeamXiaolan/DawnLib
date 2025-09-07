using System.Collections.Generic;
using System.Linq;
using Dawn;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Vehicle Definition", menuName = $"{DuskModConstants.Definitions}/Vehicle Definition")]
public class DuskVehicleDefinition : DuskContentDefinition<VehicleData, DawnVehicleInfo>, INamespaced<DuskVehicleDefinition>
{
    public const string REGISTRY_ID = "vehicles";

    [field: SerializeField]
    public string VehicleDisplayName { get; private set; }

    [field: SerializeField]
    public BuyableVehiclePreset BuyableVehiclePreset { get; private set; }

    [field: SerializeField]
    public DuskTerminalPredicate? TerminalPredicate { get; private set; }

    [field: SerializeField]
    public DuskPricingStrategy? PricingStrategy { get; private set; }

    public VehicleConfig Config { get; private set; }

    NamespacedKey<DuskVehicleDefinition> INamespaced<DuskVehicleDefinition>.TypedKey => TypedKey.AsTyped<DuskVehicleDefinition>();

    public override void Register(DuskMod mod, VehicleData data)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateVehicleConfig(section, this, data, VehicleDisplayName);

        DuskModContent.Vehicles.Register(this);
    }

    public static VehicleConfig CreateVehicleConfig(ConfigContext context, DuskVehicleDefinition definition, VehicleData data, string vehicleName)
    {
        return new VehicleConfig
        {
            DisableUnlockRequirements = data.generateDisableUnlockConfig ? context.Bind($"{vehicleName} | Disable Unlock Requirements", $"Whether {vehicleName} should have it's unlock requirements disabled.", false) : null,
            DisablePricingStrategy = data.generateDisablePricingStrategyConfig ? context.Bind($"{vehicleName} | Disable Pricing Strategy", $"Whether {vehicleName} should have it's pricing strategy disabled.", false) : null,
            Cost = context.Bind($"{vehicleName} | Cost", $"Cost for {vehicleName} in the shop.", data.cost),
        };
    }

    public override List<VehicleData> GetEntities(DuskMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.vehicles).ToList();
    }

    protected override string EntityNameReference => VehicleDisplayName;

}