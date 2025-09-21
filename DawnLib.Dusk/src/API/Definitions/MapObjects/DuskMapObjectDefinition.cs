using BepInEx.Configuration;
using Dawn;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk;

[CreateAssetMenu(fileName = "New Map Definition", menuName = $"{DuskModConstants.Definitions}/Map Object Definition")]
public class DuskMapObjectDefinition : DuskContentDefinition<DawnMapObjectInfo>
{
    [field: FormerlySerializedAs("gameObject")]
    [field: SerializeField]
    public GameObject GameObject { get; private set; }

    [field: FormerlySerializedAs("objectName")]
    [field: FormerlySerializedAs("ObjectName")]
    [field: SerializeField]
    public string MapObjectName { get; private set; }

    [field: SerializeField]
    public InsideMapObjectSettings InsideMapObjectSettings { get; private set; }
    [field: SerializeField]
    public OutsideMapObjectSettings OutsideMapObjectSettings { get; private set; }

    [field: Space(10)]
    [field: Header("Configs | Inside")]
    [field: SerializeField]
    public bool IsInsideHazard { get; private set; }
    [field: SerializeField]
    public bool CreateInsideHazardConfig { get; private set; }

    [field: SerializeField]
    public string DefaultInsideCurveSpawnWeights { get; private set; }
    [field: SerializeField]
    public bool CreateInsideCurveSpawnWeightsConfig { get; private set; }

    [field: Header("Configs | Outside")]
    [field: SerializeField]
    public bool IsOutsideHazard { get; private set; }
    [field: SerializeField]
    public bool CreateOutsideHazardConfig { get; private set; }

    [field: SerializeField]
    public string DefaultOutsideCurveSpawnWeights { get; private set; }
    [field: SerializeField]
    public bool CreateOutsideCurveSpawnWeightsConfig { get; private set; }

    public MapObjectConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateMapObjectConfig(section);

        DawnLib.DefineMapObject(TypedKey, GameObject, builder =>
        {
            if (Config.InsideHazard?.Value ?? IsInsideHazard)
            {
                MapObjectSpawnMechanics InsideSpawnMechanics = new(Config.InsideCurveSpawnWeights?.Value ?? DefaultInsideCurveSpawnWeights);
                builder.DefineInside(insideBuilder =>
                {
                    insideBuilder.OverrideSpawnFacingWall(InsideMapObjectSettings.spawnFacingWall);
                    insideBuilder.OverrideSpawnFacingAwayFromWall(InsideMapObjectSettings.spawnFacingAwayFromWall);
                    insideBuilder.OverrideRequireDistanceBetweenSpawns(InsideMapObjectSettings.requireDistanceBetweenSpawns);
                    insideBuilder.OverrideDisallowSpawningNearEntrances(InsideMapObjectSettings.disallowSpawningNearEntrances);
                    insideBuilder.OverrideSpawnWithBackToWall(InsideMapObjectSettings.spawnWithBackFlushAgainstWall);
                    insideBuilder.OverrideSpawnWithBackFlushAgainstWall(InsideMapObjectSettings.spawnWithBackFlushAgainstWall);
                    insideBuilder.SetWeights(weightBuilder =>
                    {
                        weightBuilder.SetGlobalCurve(InsideSpawnMechanics);
                    });
                });
            }

            builder.DefineOutside(outsideBuilder =>
            {
                MapObjectSpawnMechanics OutsideSpawnMechanics = new(Config.OutsideCurveSpawnWeights?.Value ?? DefaultOutsideCurveSpawnWeights);
                outsideBuilder.OverrideAlignWithTerrain(OutsideMapObjectSettings.AlignWithTerrain);
                outsideBuilder.SetWeights(weightBuilder =>
                {
                    weightBuilder.SetGlobalCurve(OutsideSpawnMechanics);
                });
            });

            ApplyTagsTo(builder);
        });
    }

    public MapObjectConfig CreateMapObjectConfig(ConfigContext section)
    {
        ConfigEntry<bool>? insideHazard = null, outsideHazard = null;
        ConfigEntry<string>? insideCurves = null, outsideCurves = null;
        if (CreateInsideHazardConfig)
        {
            insideHazard = section.Bind($"{EntityNameReference} | Is Inside Hazard", $"Whether {EntityNameReference} is an inside hazard", IsInsideHazard);
        }

        if (CreateOutsideHazardConfig)
        {
            outsideHazard = section.Bind($"{EntityNameReference} | Is Outside Hazard", $"Whether {EntityNameReference} is an outside hazard", IsOutsideHazard);
        }

        if ((insideHazard?.Value ?? IsInsideHazard) && CreateInsideCurveSpawnWeightsConfig)
        {
            insideCurves = section.Bind($"{EntityNameReference} | Inside Spawn Weights", $"Curve weights for {EntityNameReference} when spawning inside.", DefaultInsideCurveSpawnWeights);
        }

        if ((outsideHazard?.Value ?? IsOutsideHazard) && CreateOutsideCurveSpawnWeightsConfig)
        {
            outsideCurves = section.Bind($"{EntityNameReference} | Outside Spawn Weights", $"Curve weights for {EntityNameReference} when spawning outside.", DefaultOutsideCurveSpawnWeights);
        }

        return new MapObjectConfig
        {
            InsideHazard = insideHazard,
            OutsideHazard = outsideHazard,
            InsideCurveSpawnWeights = insideCurves,
            OutsideCurveSpawnWeights = outsideCurves,
        };
    }

    protected override string EntityNameReference => MapObjectName;
}