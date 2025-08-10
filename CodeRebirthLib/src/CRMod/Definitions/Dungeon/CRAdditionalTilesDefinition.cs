using System;
using System.Collections.Generic;
using System.Linq;
using DunGen;
using UnityEngine;

namespace CodeRebirthLib;

[CreateAssetMenu(fileName = "New Additional Tiles Definition", menuName = "CodeRebirthLib/Definitions/Additional Tiles Definition")]
public class CRAdditionalTilesDefinition : CRContentDefinition<DungeonData>
{
    [Flags]
    public enum BranchCapSetting
    {
        Regular = 1 << 0,
        BranchCap = 1 << 1,
    }

    public const string REGISTRY_ID = "additional_tiles";

    protected override string EntityNameReference => TilesToAdd.name;

    [field: SerializeField]
    public TileSet TilesToAdd { get; private set; }

    [field: SerializeField]
    public string ArchetypeName { get; private set; }

    [field: SerializeField]
    public BranchCapSetting BranchCap { get; private set; }

    public override void Register(CRMod mod, DungeonData data)
    {
        base.Register(mod);
        foreach (GameObjectChance chance in TilesToAdd.TileWeights.Weights)
        {
            CRLib.FixDoorwaySockets(chance.Value);
        }

        if (BranchCap.HasFlag(BranchCapSetting.Regular))
        {
            CRLib.RegisterTileSetForArchetype(ArchetypeName, TilesToAdd);
        }

        if (BranchCap.HasFlag(BranchCapSetting.BranchCap))
        {
            CRLib.RegisterTileSetForArchetype(ArchetypeName, TilesToAdd, true);
        }
    }

    public static void RegisterTo(CRMod mod)
    {
        mod.CreateRegistry(REGISTRY_ID, new CRRegistry<CRAdditionalTilesDefinition>());
    }

    public override List<DungeonData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.dungeons).ToList();
    }
}