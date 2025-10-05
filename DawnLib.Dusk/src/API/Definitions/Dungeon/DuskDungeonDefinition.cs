using Dawn;
using DunGen.Graph;
using Dusk.Weights;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Dungeon Definition", menuName = $"{DuskModConstants.Definitions}/Dungeon Definition")]
public class DuskDungeonDefinition : DuskContentDefinition<DawnDungeonInfo>
{
    [field: SerializeField]
    public DungeonFlow DungeonFlow { get; private set; }
    [field: SerializeField]
    public AudioClip? StingerAudio { get; private set; }

    [field: Space(10)]
    [field: Header("Config | Weights")]
    [field: SerializeField]
    public string MoonSpawnWeights { get; private set; }

    [field: Header("Config | Misc")]
    [field: SerializeField]
    public float MapTileSize { get; private set; }
    [field: SerializeField]
    public bool StingerPlaysMoreThanOnce { get; private set; }
    [field: SerializeField]
    public float StingerPlayChance { get; private set; }

    public DungeonConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);

        SpawnWeightsPreset spawnWeightsPreset = new();
        spawnWeightsPreset.SetupSpawnWeightsPreset(MoonSpawnWeights, string.Empty, string.Empty);
        DawnLib.DefineDungeon(TypedKey, DungeonFlow, builder =>
        {
            ApplyTagsTo(builder);
            builder.SetMapTileSize(MapTileSize);
            builder.SetFirstTimeAudio(StingerAudio);
            builder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(spawnWeightsPreset));
        });
    }

    protected override string EntityNameReference => DungeonFlow?.name ?? string.Empty;
}