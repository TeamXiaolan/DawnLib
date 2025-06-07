using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;
using LethalLib.Extras;
using LethalLib.Modules;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.Unlockables;

[CreateAssetMenu(fileName = "New Unlockable Definition", menuName = "CodeRebirthLib/Unlockable Definition")]
public class CRUnlockableDefinition : CRContentDefinition<UnlockableData>
{
    [field: FormerlySerializedAs("unlockableItemDef"), SerializeField]
    public UnlockableItemDef UnlockableItemDef { get; private set; }

    [field: FormerlySerializedAs("DenyPurchaseNode"), SerializeField]
    public TerminalNode? DenyPurchaseNode { get; private set; }

    public UnlockableConfig Config { get; private set; }
    
    public override void Register(CRMod mod, UnlockableData data)
    {
        Config = CreateUnlockableConfig(mod, data, UnlockableItemDef.unlockable.unlockableName);

        if (Config.IsShipUpgrade.Value)
        {
            LethalLib.Modules.Unlockables.RegisterUnlockable(UnlockableItemDef, Config.Cost.Value, StoreType.ShipUpgrade);
        }
        
        if (Config.IsDecor.Value)
        {
            LethalLib.Modules.Unlockables.RegisterUnlockable(UnlockableItemDef, Config.Cost.Value, StoreType.Decor);
        }

        if (Config.IsProgressive?.Value ?? data.isProgressive)
        {
            // todo
        }
        
        mod.Unlockables.Register(this);
    }

    public static UnlockableConfig CreateUnlockableConfig(CRMod mod, UnlockableData data, string unlockableName)
    {
        using(ConfigContext context = mod.ConfigManager.CreateConfigSection(unlockableName))
        {
            return new UnlockableConfig()
            {
                IsProgressive = data.createProgressiveConfig ? context.Bind("Is Progressive", $"Whether {unlockableName} is considered a progressive purchase.", data.isProgressive) : null,
                IsDecor = context.Bind("Is Decor", $"Whether {unlockableName} is considered a decor.", data.isDecor),
                IsShipUpgrade = context.Bind("Is Ship Upgrade", $"Whether {unlockableName} is considered a ship upgrade.", data.isShipUpgrade),
                Cost = context.Bind("Cost", $"Csot for {unlockableName} in the shop.", data.cost)
            };
        }
    }
    
    public override List<UnlockableData> GetEntities(CRMod mod) => mod.Content.assetBundles.SelectMany(it => it.unlockables).ToList();
}