using System;
using System.Collections.Generic;
using System.Linq;
using DunGen;
using UnityEngine;

namespace Dawn.Dusk;

[CreateAssetMenu(fileName = "New Additional Tiles Definition", menuName = $"{CRModConstants.Definitions}/Additional Tiles Definition")]
public class CRMAdditionalTilesDefinition : CRMContentDefinition<DungeonData, CRTileSetInfo>
{
    [Flags]
    public enum BranchCapSetting
    {
        Regular = 1 << 0,
        BranchCap = 1 << 1,
    }

    public const string REGISTRY_ID = "additional_tiles";

    [field: SerializeField]
    public TileSet TilesToAdd { get; private set; }

    [field: SerializeField]
    public List<NamespacedKey<CRArchetypeInfo>> archetypeKeys = new();

    [field: SerializeField]
    public BranchCapSetting BranchCap { get; private set; }

    private CRTileSetInfo _info;
    
    public override void Register(CRMod mod, DungeonData data)
    {
        base.Register(mod);
        foreach (GameObjectChance chance in TilesToAdd.TileWeights.Weights)
        {
            CRLib.FixDoorwaySockets(chance.Value);
        }

        _info = CRLib.DefineTileSet(TypedKey, TilesToAdd, builder =>
        {
            ApplyTagsTo(builder);
            builder.SetIsRegular(BranchCap.HasFlag(BranchCapSetting.Regular));
            builder.SetIsBranchCap(BranchCap.HasFlag(BranchCapSetting.BranchCap));
        });

        LethalContent.Archetypes.BeforeFreeze += () =>
        {
            foreach (NamespacedKey<CRArchetypeInfo> key in archetypeKeys)
            {
                if (LethalContent.Archetypes.TryGetValue(key, out CRArchetypeInfo dungeonInfo))
                {
                    dungeonInfo.AddTileSet(_info);
                }
            }
        };
    }

    public override List<DungeonData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.dungeons).ToList();
    }

    protected override string EntityNameReference => TilesToAdd.name;
}