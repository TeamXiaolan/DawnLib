using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib;
[CreateAssetMenu(fileName = "New Unlockable Definition", menuName = "CodeRebirthLib/Definitions/Unlockable Definition")]
public class CRUnlockableDefinition : CRContentDefinition<UnlockableData>
{
    public const string REGISTRY_ID = "unlockables";

    [field: SerializeField]
    public UnlockableItem UnlockableItem { get; private set; }

    [field: FormerlySerializedAs("DenyPurchaseNode")]
    [field: SerializeField]
    public TerminalNode? ProgressiveDenyNode { get; private set; }

    public UnlockableConfig Config { get; private set; }

    public ProgressiveUnlockData? ProgressiveData { get; private set; }

    protected override string EntityNameReference => UnlockableItem.unlockableName;

    public override void Register(CRMod mod, UnlockableData data)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateUnlockableConfig(section, data, UnlockableItem.unlockableName);

        if (Config.IsShipUpgrade.Value)
        {
            UnlockableItem.alwaysInStock = true;
        }
        else if (Config.IsDecor.Value)
        {
            UnlockableItem.alwaysInStock = false;
        }

        CRLib.DefineUnlockable(null, UnlockableItem, builder =>
        {
            builder.SetCost(Config.Cost.Value);
            builder.DefineShop(shopBuilder =>
            {
                shopBuilder.Build();
            });
        });

        if (Config.IsProgressive?.Value ?? data.isProgressive)
        {
            if (!ProgressiveDenyNode)
            {
                ProgressiveDenyNode = CreateDefaultProgressiveDenyNode();
            }

            Debuggers.ReplaceThis?.Log($"Creating ProgressiveUnlockData for {UnlockableItem.unlockableName}");
            ProgressiveData = new ProgressiveUnlockData(this);
        }
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

    private static TerminalNode CreateDefaultProgressiveDenyNode()
    {
        TerminalNode node = CreateInstance<TerminalNode>();
        node.displayText = "Ship Upgrade or Decor is not unlocked";
        return node;
    }

    public override List<UnlockableData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.unlockables).ToList();
    }
}