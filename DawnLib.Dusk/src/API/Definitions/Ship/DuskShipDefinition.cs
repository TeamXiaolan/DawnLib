using Dawn;
using Dawn.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Ship Definition", menuName = $"{DuskModConstants.Definitions}/Ship Definition")]
public class DuskShipDefinition : DuskContentDefinition<DawnShipInfo>, INamespaced<DuskShipDefinition>
{
    [field: SerializeField]
    public BuyableShipPreset BuyableShipPreset { get; private set; }

    [field: SerializeField]
    public DuskTerminalPredicate? TerminalPredicate { get; private set; }

    [field: SerializeField]
    public DuskPricingStrategy? PricingStrategy { get; private set; }

    [field: Header("Configs | Misc")]
    [field: SerializeField]
    public bool GenerateDisableUnlockConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateDisablePricingStrategyConfig { get; private set; } = true;

    protected override string EntityNameReference => BuyableShipPreset.ShipName;

    public ShipConfig Config { get; private set; }
    public DawnShipInfo DawnShipInfo { get; private set; }
    NamespacedKey<DuskShipDefinition> INamespaced<DuskShipDefinition>.TypedKey => TypedKey.AsTyped<DuskShipDefinition>();

    public override void Register(DuskMod mod)
    {
        if (!BuyableShipPreset.ShipPrefab)
        {
            DuskPlugin.Logger.LogError($"The ship: {BuyableShipPreset.ShipName} has no ship prefab.");
            return;
        }

        if (!BuyableShipPreset.NavmeshPrefab)
        {
            DuskPlugin.Logger.LogError($"The ship: {BuyableShipPreset.ShipName} has no ship navmesh prefab.");
            return;
        }

        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateShipConfig(section);

        BuyableShipPreset.Cost = Config.Cost.Value;
        if (Config.DisableUnlockRequirements?.Value ?? false && TerminalPredicate)
        {
            TerminalPredicate = null;
        }

        if (Config.DisablePricingStrategy?.Value ?? false && PricingStrategy)
        {
            PricingStrategy = null;
        }

        DawnShipInfo = new DawnShipInfo(
            TypedKey,
            _tags.ToHashSet(),
            BuyableShipPreset.ShipName,
            BuyableShipPreset.ShipPrefab,
            BuyableShipPreset.NavmeshPrefab,
            BuyableShipPreset.Cost,
            null
        );

        //i THINK i should do mess with it and add to BUY command somehow but head hurts
        TerminalCommandBasicInformation commandBaseInfo = new($"{BuyableShipPreset.ShipName}Query", "ship category", "test ship api", ClearText.Query);
        NamespacedKey<DawnTerminalCommandInfo> namespacedKey = NamespacedKey<DawnTerminalCommandInfo>.From($"{TypedKey.Namespace}", $"{BuyableShipPreset.ShipName}QueryCommand");
        DawnLib.DefineTerminalCommand(namespacedKey, commandBaseInfo, builder =>
        {
            builder.SetKeywords(new List<string>([BuyableShipPreset.ShipName]));
            builder.DefineSimpleQueryCommand(queryCommandBuilder =>
            {
                queryCommandBuilder.SetContinueOrCancel(() => $"Are you sure you want to buy {BuyableShipPreset.ShipName}?"); //add confirm or deny text
                queryCommandBuilder.SetCancel(() => ""); //change that
                queryCommandBuilder.SetContinueWord("confirm");
                queryCommandBuilder.SetCancelWord("deny");
                queryCommandBuilder.SetQueryEvent((bool value) => ShipSpawnHandler.Instance.ChangeShip(Key));
                queryCommandBuilder.SetResult(() => "wider!");
            });
        });

        //TODO: prob should move it to dawn
        LethalContent.Ships.Register(DawnShipInfo);

        DuskModContent.Ships.Register(this);
    }

    public override void TryNetworkRegisterAssets()
    {
        if (BuyableShipPreset.ShipPrefab != null && BuyableShipPreset.ShipPrefab.TryGetComponent(out NetworkObject _))
            DawnLib.RegisterNetworkPrefab(BuyableShipPreset.ShipPrefab);
        else
            DawnPlugin.Logger.LogError($"{BuyableShipPreset.ShipName} ShipPrefab does not have NetworkObject! This will cause issues.");
    }

    public ShipConfig CreateShipConfig(ConfigContext context)
    {
        return new ShipConfig
        {
            DisableUnlockRequirements = GenerateDisableUnlockConfig && TerminalPredicate ? context.Bind($"{EntityNameReference} | Disable Unlock Requirements", $"Whether {EntityNameReference} should have it's unlock requirements disabled.", false) : null,
            DisablePricingStrategy = GenerateDisablePricingStrategyConfig && PricingStrategy ? context.Bind($"{EntityNameReference} | Disable Pricing Strategy", $"Whether {EntityNameReference} should have it's pricing strategy disabled.", false) : null,
            Cost = context.Bind($"{EntityNameReference} | Cost", $"Cost for {EntityNameReference} in the shop.", BuyableShipPreset.Cost)
        };
    }
}
