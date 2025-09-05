using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Unlockable Definition", menuName = $"{DuskModConstants.Definitions}/Unlockable Definition")]
public class DuskUnlockableDefinition : DuskContentDefinition<UnlockableData, DawnUnlockableItemInfo>
{
    public const string REGISTRY_ID = "unlockables";

    [field: SerializeField]
    public UnlockableItem UnlockableItem { get; private set; }

    [field: SerializeField]
    public DuskTerminalPredicate TerminalPredicate { get; private set; }
    
    [field: SerializeField]
    public DuskPricingStrategy PricingStrategy { get; private set; }

    public UnlockableConfig Config { get; private set; }

    public override void Register(DuskMod mod, UnlockableData data)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateUnlockableConfig(section, this, data, UnlockableItem.unlockableName);
    
        DawnLib.DefineUnlockable(TypedKey, UnlockableItem, builder =>
        {
            bool disablePricingStrategy = Config.DisablePricingStrategy?.Value ?? false;
            if (PricingStrategy && !disablePricingStrategy)
            {
                PricingStrategy.Register(Key);
                builder.SetCost(PricingStrategy);
            }
            else
            {
                builder.SetCost(Config.Cost.Value);
            }
            builder.DefineShop(shopBuilder =>
            {
                shopBuilder.Build();
                if (Config.IsShipUpgrade?.Value ?? data.isShipUpgrade)
                {
                    Debuggers.Unlockables?.Log($"Making {UnlockableItem.unlockableName} a Ship Upgrade");
                    shopBuilder.SetShipUpgrade();
                }
                else if (Config.IsDecor?.Value ?? data.isDecor)
                {
                    Debuggers.Unlockables?.Log($"Making {UnlockableItem.unlockableName} a Decor");
                    shopBuilder.SetDecor();
                }
            });

            bool disableUnlockRequirements = Config.DisableUnlockRequirement?.Value ?? false;
            if (!disableUnlockRequirements && TerminalPredicate)
            {
                TerminalPredicate.Register(UnlockableItem.unlockableName);
                builder.SetPurchasePredicate(TerminalPredicate);
            }

            ApplyTagsTo(builder);
        });
    }

    public static UnlockableConfig CreateUnlockableConfig(ConfigContext context, DuskUnlockableDefinition definition, UnlockableData data, string unlockableName)
    {
        return new UnlockableConfig
        {
            DisablePricingStrategy = data.generateDisablePricingStrategyConfig ? context.Bind($"{unlockableName} | Disable Pricing Strategy", $"Whether {unlockableName} should have it's pricing strategy disabled.", false) : null,
            DisableUnlockRequirement = data.generateDisableUnlockRequirementConfig ? context.Bind($"{unlockableName} | Disable Unlock Requirements", $"Whether {unlockableName} should have it's unlock requirements disabled.", false) : null,
            IsDecor = context.Bind($"{unlockableName} | Is Decor", $"Whether {unlockableName} is considered a decor.", data.isDecor),
            IsShipUpgrade = context.Bind($"{unlockableName} | Is Ship Upgrade", $"Whether {unlockableName} is considered a ship upgrade.", data.isShipUpgrade),
            Cost = context.Bind($"{unlockableName} | Cost", $"Cost for {unlockableName} in the shop.", data.cost),
        };
    }

    public override List<UnlockableData> GetEntities(DuskMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.unlockables).ToList();
    }

    protected override string EntityNameReference => UnlockableItem.unlockableName;
}