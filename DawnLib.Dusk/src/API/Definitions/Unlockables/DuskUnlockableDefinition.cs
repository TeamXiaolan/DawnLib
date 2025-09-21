using Dawn;
using Dawn.Internal;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Unlockable Definition", menuName = $"{DuskModConstants.Definitions}/Unlockable Definition")]
public class DuskUnlockableDefinition : DuskContentDefinition<DawnUnlockableItemInfo>
{
    [field: SerializeField]
    public UnlockableItem UnlockableItem { get; private set; } = new();

    [field: SerializeField]
    public string InfoNodeText { get; private set; } = string.Empty;

    [field: SerializeField]
    public DuskTerminalPredicate TerminalPredicate { get; private set; }

    [field: SerializeField]
    public DuskPricingStrategy PricingStrategy { get; private set; }

    [field: Space(10)]
    [field: Header("Configs | Main")]
    [field: SerializeField]
    public bool IsShipUpgrade { get; private set; }
    [field: SerializeField]
    public bool IsDecor { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }

    [field: Header("Configs | Misc")]
    [field: SerializeField]
    public bool GenerateDisablePricingStrategyConfig { get; private set; }
    [field: SerializeField]
    public bool GenerateDisableUnlockRequirementConfig { get; private set; }

    public UnlockableConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateUnlockableConfig(section);

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

            if (UnlockableItem.prefabObject != null)
            {
                builder.DefinePlaceableObject(shopBuilder =>
                {
                    shopBuilder.Build();
                    if (Config.IsShipUpgrade?.Value ?? IsShipUpgrade)
                    {
                        Debuggers.Unlockables?.Log($"Making {UnlockableItem.unlockableName} a Ship Upgrade");
                        shopBuilder.SetShipUpgrade();
                    }
                    else if (Config.IsDecor?.Value ?? IsDecor)
                    {
                        Debuggers.Unlockables?.Log($"Making {UnlockableItem.unlockableName} a Decor");
                        shopBuilder.SetDecor();
                    }
                });
            }

            if (UnlockableItem.suitMaterial != null)
            {
                builder.DefineSuit(suitBuilder =>
                {
                    suitBuilder.OverrideSuitMaterial(UnlockableItem.suitMaterial);
                    suitBuilder.OverrideJumpAudioClip(UnlockableItem.jumpAudio);
                    suitBuilder.Build();
                });
            }

            bool disableUnlockRequirements = Config.DisableUnlockRequirement?.Value ?? false;
            if (!disableUnlockRequirements && TerminalPredicate)
            {
                TerminalPredicate.Register(Key);
                builder.SetPurchasePredicate(TerminalPredicate);
            }

            builder.SetInfoText(InfoNodeText);

            ApplyTagsTo(builder);
        });
    }

    public UnlockableConfig CreateUnlockableConfig(ConfigContext context)
    {
        string unlockableName = UnlockableItem.unlockableName;
        return new UnlockableConfig
        {
            DisablePricingStrategy = GenerateDisablePricingStrategyConfig && PricingStrategy ? context.Bind($"{unlockableName} | Disable Pricing Strategy", $"Whether {unlockableName} should have it's pricing strategy disabled.", false) : null,
            DisableUnlockRequirement = GenerateDisableUnlockRequirementConfig && TerminalPredicate ? context.Bind($"{unlockableName} | Disable Unlock Requirements", $"Whether {unlockableName} should have it's unlock requirements disabled.", false) : null,
            IsDecor = context.Bind($"{unlockableName} | Is Decor", $"Whether {unlockableName} is considered a decor.", IsDecor),
            IsShipUpgrade = context.Bind($"{unlockableName} | Is Ship Upgrade", $"Whether {unlockableName} is considered a ship upgrade.", IsShipUpgrade),
            Cost = context.Bind($"{unlockableName} | Cost", $"Cost for {unlockableName} in the shop.", Cost),
        };
    }

    protected override string EntityNameReference => UnlockableItem.unlockableName;
}