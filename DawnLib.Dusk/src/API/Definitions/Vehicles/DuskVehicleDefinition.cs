using System.Linq;
using Dawn;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Vehicle Definition", menuName = $"{DuskModConstants.Definitions}/Vehicle Definition")]
public class DuskVehicleDefinition : DuskContentDefinition<DawnVehicleInfo>, INamespaced<DuskVehicleDefinition>
{
    [field: SerializeField]
    public string VehicleDisplayName { get; private set; }

    [field: SerializeField]
    public BuyableVehiclePreset BuyableVehiclePreset { get; private set; }

    [field: SerializeField]
    public DuskTerminalPredicate? TerminalPredicate { get; private set; }

    [field: SerializeField]
    public DuskPricingStrategy? PricingStrategy { get; private set; }

    [field: Space(10)]
    [field: Header("Configs | Main")]
    [field: SerializeField]
    public int Cost { get; private set; }

    [field: Header("Configs | Misc")]
    [field: SerializeField]
    public bool GenerateDisableUnlockConfig { get; private set; }
    [field: SerializeField]
    public bool GenerateDisablePricingStrategyConfig { get; private set; }

    public VehicleConfig Config { get; private set; }
    public DawnVehicleInfo DawnVehicleInfo { get; private set; }

    NamespacedKey<DuskVehicleDefinition> INamespaced<DuskVehicleDefinition>.TypedKey => TypedKey.AsTyped<DuskVehicleDefinition>();

    public override void Register(DuskMod mod)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateVehicleConfig(section);

        Cost = Config.Cost.Value;
        if (Config.DisableUnlockRequirements?.Value ?? false && TerminalPredicate)
        {
            TerminalPredicate = null;
        }

        if (Config.DisablePricingStrategy?.Value ?? false && PricingStrategy)
        {
            PricingStrategy = null;
        }
        DawnVehicleInfo = new DawnVehicleInfo(TerminalPredicate ?? ITerminalPurchasePredicate.AlwaysSuccess(), TypedKey, _tags.ToHashSet(), BuyableVehiclePreset.VehiclePrefab, BuyableVehiclePreset.SecondaryPrefab, BuyableVehiclePreset.StationPrefab, PricingStrategy == null ? new SimpleProvider<int>(Cost) : PricingStrategy, null);
        DuskModContent.Vehicles.Register(this);
    }

    public VehicleConfig CreateVehicleConfig(ConfigContext context)
    {
        return new VehicleConfig
        {
            DisableUnlockRequirements = GenerateDisableUnlockConfig && TerminalPredicate ? context.Bind($"{EntityNameReference} | Disable Unlock Requirements", $"Whether {EntityNameReference} should have it's unlock requirements disabled.", false) : null,
            DisablePricingStrategy = GenerateDisablePricingStrategyConfig && PricingStrategy ? context.Bind($"{EntityNameReference} | Disable Pricing Strategy", $"Whether {EntityNameReference} should have it's pricing strategy disabled.", false) : null,
            Cost = context.Bind($"{EntityNameReference} | Cost", $"Cost for {EntityNameReference} in the shop.", Cost),
        };
    }

    protected override string EntityNameReference => VehicleDisplayName;
}