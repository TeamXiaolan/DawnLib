using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Map Definition", menuName = "CodeRebirthLib/Definitions/Map Object Definition")]
public class CRMMapObjectDefinition : CRMContentDefinition<MapObjectData, CRMapObjectInfo>
{
    public const string REGISTRY_ID = "map_objects";

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

    public MapObjectConfig Config { get; private set; }
    protected override string EntityNameReference => MapObjectName;



    public override void Register(CRMod mod, MapObjectData data)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateMapObjectConfig(section, data, MapObjectName);

        CRLib.DefineMapObject(TypedKey, GameObject, builder =>
        {
            if (Config.InsideHazard?.Value ?? data.isInsideHazard)
            {
                MapObjectSpawnMechanics InsideSpawnMechanics = new(Config.InsideCurveSpawnWeights?.Value ?? data.defaultInsideCurveSpawnWeights);
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
                MapObjectSpawnMechanics OutsideSpawnMechanics = new(Config.OutsideCurveSpawnWeights?.Value ?? data.defaultOutsideCurveSpawnWeights);
                outsideBuilder.OverrideAlignWithTerrain(OutsideMapObjectSettings.AlignWithTerrain);
                outsideBuilder.SetWeights(weightBuilder =>
                {
                    weightBuilder.SetGlobalCurve(OutsideSpawnMechanics);
                });
            });
        });
    }

    public static MapObjectConfig CreateMapObjectConfig(ConfigContext section, MapObjectData data, string objectName)
    {
        ConfigEntry<bool>? insideHazard = null, outsideHazard = null;
        ConfigEntry<string>? insideCurves = null, outsideCurves = null;
        if (data.createInsideHazardConfig)
            insideHazard = section.Bind($"{objectName} | Is Inside Hazard", $"Whether {objectName} is an inside hazard", data.isInsideHazard);

        if (data.createOutsideHazardConfig)
            outsideHazard = section.Bind($"{objectName} | Is Outside Hazard", $"Whether {objectName} is an outside hazard", data.isOutsideHazard);

        if ((insideHazard?.Value ?? data.isInsideHazard) && data.createInsideCurveSpawnWeightsConfig)
            insideCurves = section.Bind($"{objectName} | Inside Spawn Weights", $"Curve weights for {objectName} when spawning inside.", data.defaultInsideCurveSpawnWeights);

        if ((outsideHazard?.Value ?? data.isOutsideHazard) && data.createOutsideCurveSpawnWeightsConfig)
            outsideCurves = section.Bind($"{objectName} | Outside Spawn Weights", $"Curve weights for {objectName} when spawning outside.", data.defaultOutsideCurveSpawnWeights);

        return new MapObjectConfig
        {
            InsideHazard = insideHazard,
            OutsideHazard = outsideHazard,
            InsideCurveSpawnWeights = insideCurves,
            OutsideCurveSpawnWeights = outsideCurves,
        };
    }

    public override List<MapObjectData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.mapObjects).ToList();
    }

    public override string GetDefaultKey()
    {
        return MapObjectName;
    }
}