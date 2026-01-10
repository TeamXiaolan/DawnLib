using System.Linq;
using Dawn;
using Dawn.Internal;
using Unity.Netcode;
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
    public bool GenerateDisableUnlockConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateDisablePricingStrategyConfig { get; private set; } = true;

    public VehicleConfig Config { get; private set; }
    public DawnVehicleInfo DawnVehicleInfo { get; private set; }

    NamespacedKey<DuskVehicleDefinition> INamespaced<DuskVehicleDefinition>.TypedKey => TypedKey.AsTyped<DuskVehicleDefinition>();

    public override void Register(DuskMod mod)
    {
        if (BuyableVehiclePreset.StationPrefab != null)
        {
            if (BuyableVehiclePreset.StationPrefab.GetComponent<AutoParentToShip>() == null)
            {
                DuskPlugin.Logger.LogError($"The vehicle: {BuyableVehiclePreset.StationPrefab.name} has no {nameof(AutoParentToShip)} component.");
                return;
            }

            if (BuyableVehiclePreset.StationPrefab.GetComponentsInChildren<PlaceableShipObject>() == null)
            {
                DuskPlugin.Logger.LogError($"The vehicle: {BuyableVehiclePreset.StationPrefab.name} has no {nameof(PlaceableShipObject)} component.");
                return;
            }

            NamespacedKey stationKey = BuyableVehiclePreset.StationPrefab.GetComponent<StationBase>().StationKey;
            UnlockableItem unlockableItem = new UnlockableItem()
            {
                unlockableName = $"{stationKey.Key} (Station)",
                prefabObject = BuyableVehiclePreset.StationPrefab,
                unlockableType = 1,
                alwaysInStock = true,
                IsPlaceable = true
            };
            DawnLib.DefineUnlockable(stationKey.AsTyped<DawnUnlockableItemInfo>(), unlockableItem, builder =>
            {
                builder.SetCost(99999);

                builder.DefinePlaceableObject(shopBuilder =>
                {
                    Debuggers.Unlockables?.Log($"Making {unlockableItem.unlockableName} a Ship Upgrade");
                    shopBuilder.SetShipUpgrade();
                });

                ITerminalPurchasePredicate purchasePredicate = ITerminalPurchasePredicate.AlwaysHide();
                builder.SetPurchasePredicate(purchasePredicate);
            });
        }

        base.Register(mod);
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

        DawnVehicleInfo = new DawnVehicleInfo(TypedKey, _tags.ToHashSet(), BuyableVehiclePreset.VehiclePrefab, BuyableVehiclePreset.SecondaryPrefab, BuyableVehiclePreset.StationPrefab, new DawnPurchaseInfo(PricingStrategy == null ? new SimpleProvider<int>(Cost) : PricingStrategy, TerminalPredicate ?? ITerminalPurchasePredicate.AlwaysSuccess()), null);
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

    public override void TryNetworkRegisterAssets()
    {
        if (BuyableVehiclePreset.VehiclePrefab != null && BuyableVehiclePreset.VehiclePrefab.TryGetComponent(out NetworkObject _))
        {
            DawnLib.RegisterNetworkPrefab(BuyableVehiclePreset.VehiclePrefab);
        }

        if (BuyableVehiclePreset.SecondaryPrefab != null && BuyableVehiclePreset.SecondaryPrefab.TryGetComponent(out NetworkObject _))
        {
            DawnLib.RegisterNetworkPrefab(BuyableVehiclePreset.SecondaryPrefab);
        }

        if (BuyableVehiclePreset.StationPrefab != null && BuyableVehiclePreset.StationPrefab.TryGetComponent(out NetworkObject _))
        {
            DawnLib.RegisterNetworkPrefab(BuyableVehiclePreset.StationPrefab);
        }
    }

    protected override string EntityNameReference => VehicleDisplayName;
}