using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Internal;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Unlockable Definition", menuName = "CodeRebirthLib/Definitions/Unlockable Definition")]
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
        Config = CreateUnlockableConfig(section, data, UnlockableItem.unlockableName);

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

            // todo: probably should be better, because CRMTerminalPredicate can really be anything.
            // config will need to be updated though
            if (Config.IsProgressive?.Value ?? data.isProgressive)
            {
                Debuggers.Progressive?.Log($"Creating ProgressiveUnlockData for {UnlockableItem.unlockableName}");
                if (!TerminalPredicate)
                    TerminalPredicate = ScriptableObject.CreateInstance<ProgressivePredicate>();

                TerminalPredicate.Register(UnlockableItem.unlockableName);
                builder.SetPurchasePredicate(TerminalPredicate);
            }

            foreach (NamespacedKey tag in _tags)
            {
                builder.AddTag(tag);
            }
        });
    }

    public static UnlockableConfig CreateUnlockableConfig(ConfigContext context, UnlockableData data, string unlockableName)
    {
        return new UnlockableConfig
        {
            IsProgressive = data.createProgressiveConfig ? context.Bind($"{unlockableName} | Is Progressive", $"Whether {unlockableName} is considered a progressive purchase.", data.isProgressive) : null,
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