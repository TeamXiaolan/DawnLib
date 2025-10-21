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
    public bool GenerateUnlockableTypeConfig { get; private set; }
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
        base.Register(mod);
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
        return new UnlockableConfig
        {
            DisablePricingStrategy = GenerateDisablePricingStrategyConfig && PricingStrategy ? context.Bind($"{EntityNameReference} | Disable Pricing Strategy", $"Whether {EntityNameReference} should have it's pricing strategy disabled.", false) : null,
            DisableUnlockRequirement = GenerateDisableUnlockRequirementConfig && TerminalPredicate ? context.Bind($"{EntityNameReference} | Disable Unlock Requirements", $"Whether {EntityNameReference} should have it's unlock requirements disabled.", false) : null,
            IsDecor = GenerateUnlockableTypeConfig ? context.Bind($"{EntityNameReference} | Is Decor", $"Whether {EntityNameReference} is considered a decor.", IsDecor) : null,
            IsShipUpgrade = GenerateUnlockableTypeConfig ? context.Bind($"{EntityNameReference} | Is Ship Upgrade", $"Whether {EntityNameReference} is considered a ship upgrade.", IsShipUpgrade) : null,
            Cost = context.Bind($"{EntityNameReference} | Cost", $"Cost for {EntityNameReference} in the shop.", Cost),
        };
    }

    protected override string EntityNameReference => UnlockableItem.unlockableName;
}