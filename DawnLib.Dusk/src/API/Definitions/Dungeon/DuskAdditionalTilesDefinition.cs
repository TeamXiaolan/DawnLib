using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using DunGen;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Additional Tiles Definition", menuName = $"{DuskModConstants.Definitions}/Additional Tiles Definition")]
public class DuskAdditionalTilesDefinition : DuskContentDefinition<DungeonData, DawnTileSetInfo>
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
    public List<NamespacedKey<DawnArchetypeInfo>> archetypeKeys = new();

    [field: SerializeField]
    public BranchCapSetting BranchCap { get; private set; }

    private DawnTileSetInfo _info;

    public override void Register(DuskMod mod, DungeonData data)
    {
        base.Register(mod);
        foreach (GameObjectChance chance in TilesToAdd.TileWeights.Weights)
        {
            DawnLib.FixDoorwaySockets(chance.Value);
        }

        _info = DawnLib.DefineTileSet(TypedKey, TilesToAdd, builder =>
        {
            ApplyTagsTo(builder);
            builder.SetIsRegular(BranchCap.HasFlag(BranchCapSetting.Regular));
            builder.SetIsBranchCap(BranchCap.HasFlag(BranchCapSetting.BranchCap));
        });

        LethalContent.Archetypes.BeforeFreeze += () =>
        {
            foreach (NamespacedKey<DawnArchetypeInfo> key in archetypeKeys)
            {
                if (LethalContent.Archetypes.TryGetValue(key, out DawnArchetypeInfo dungeonInfo))
                {
                    dungeonInfo.AddTileSet(_info);
                }
            }
        };
    }

    public override List<DungeonData> GetEntities(DuskMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.dungeons).ToList();
    }

    protected override string EntityNameReference => TilesToAdd.name;
}