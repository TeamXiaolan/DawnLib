using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Internal;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Unlockable Definition", menuName = $"{CRModConstants.Definitions}/Unlockable Definition")]
public class CRMUnlockableDefinition : CRMContentDefinition<UnlockableData, CRUnlockableItemInfo>
{
    public const string REGISTRY_ID = "unlockables";

    [field: SerializeField]
    public UnlockableItem UnlockableItem { get; private set; }

    [field: SerializeField]
    public CRMTerminalPredicate TerminalPredicate { get; private set; }

    public UnlockableConfig Config { get; private set; }

    public override void Register(CRMod mod, UnlockableData data)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateUnlockableConfig(section, this, data, UnlockableItem.unlockableName);

        CRLib.DefineUnlockable(TypedKey, UnlockableItem, builder =>
        {
            builder.SetCost(Config.Cost.Value);
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

    public static UnlockableConfig CreateUnlockableConfig(ConfigContext context, CRMUnlockableDefinition definition, UnlockableData data, string unlockableName)
    {
        return new UnlockableConfig
        {
            DisableUnlockRequirement = data.generateDisableUnlockRequirementConfig ? context.Bind($"{unlockableName} | Disable Unlock Requirements", $"Whether {unlockableName} should have it's unlock requirements disabled.", definition.TerminalPredicate != null) : null,
            IsDecor = context.Bind($"{unlockableName} | Is Decor", $"Whether {unlockableName} is considered a decor.", data.isDecor),
            IsShipUpgrade = context.Bind($"{unlockableName} | Is Ship Upgrade", $"Whether {unlockableName} is considered a ship upgrade.", data.isShipUpgrade),
            Cost = context.Bind($"{unlockableName} | Cost", $"Cost for {unlockableName} in the shop.", data.cost),
        };
    }

    public override List<UnlockableData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.unlockables).ToList();
    }

    protected override string EntityNameReference => UnlockableItem.unlockableName;
}