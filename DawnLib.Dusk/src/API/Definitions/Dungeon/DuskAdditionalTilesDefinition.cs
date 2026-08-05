using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using DunGen;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Additional Tiles Definition", menuName = $"{DuskModConstants.Definitions}/Additional Tiles Definition")]
public class DuskAdditionalTilesDefinition : DuskContentDefinition<DawnTileSetInfo>
{
    [Flags]
    public enum BranchCapSetting
    {
        Regular = 1 << 0,
        BranchCap = 1 << 1,
    }

    [field: SerializeField]
    public TileSet TilesToAdd { get; private set; }

    [field: SerializeField]
    [field: UnlockedNamespacedKey]
    public List<NamespacedKey<DawnArchetypeInfo>> archetypeKeys = new();

    [field: SerializeField]
    public BranchCapSetting BranchCap { get; private set; } = BranchCapSetting.Regular | BranchCapSetting.BranchCap;

    [field: SerializeField]
    public DuskPredicate? Predicate { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        DawnTileSetInfo tileSetInfo = DawnLib.DefineTileSet(TypedKey, TilesToAdd, builder =>
        {
            ApplyTagsTo(builder);
            builder.SetIsRegular(BranchCap.HasFlag(BranchCapSetting.Regular));
            builder.SetIsBranchCap(BranchCap.HasFlag(BranchCapSetting.BranchCap));
            if (Predicate != null)
            {
                builder.SetInjectionPredicate(Predicate);
            }
        });

        tileSetInfo.SetupDetails();
        tileSetInfo.RegisterSpawnSyncedObjects();

        LethalContent.Archetypes.BeforeFreezeWithContext += _ =>
        {
            foreach (NamespacedKey<DawnArchetypeInfo> key in archetypeKeys)
            {
                if (LethalContent.Archetypes.TryGetValue(key, out DawnArchetypeInfo archetypeInfo))
                {
                    archetypeInfo.AddTileSet(tileSetInfo);
                }
            }
        };
    }

    public override void TryNetworkRegisterAssets()
    {
        foreach (GameObject gameObject in TilesToAdd.TileWeights.Weights.Select(x => x.Value))
        {
            if (!gameObject.TryGetComponent(out NetworkObject _))
                continue;

            DawnLib.RegisterNetworkPrefab(gameObject);
        }
    }
    protected override string EntityNameReference => TilesToAdd?.name ?? string.Empty;
}