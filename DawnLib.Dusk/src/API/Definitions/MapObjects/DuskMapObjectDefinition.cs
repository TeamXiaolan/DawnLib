using System.Collections.Generic;
using BepInEx.Configuration;
using Dawn;
using Unity.Netcode;
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
    public List<NamespacedKeyWithAnimationCurve> InsideMoonCurveSpawnWeights { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedKeyWithAnimationCurve> InsideInteriorCurveSpawnWeights { get; private set; } = new();
    [field: SerializeField]
    public bool InsidePrioritiseMoonConfig { get; private set; } = true;

    [field: SerializeField]
    public bool CreateInsideCurveSpawnWeightsConfig { get; private set; }

    [field: Header("Configs | Outside")]
    [field: SerializeField]
    public bool IsOutsideHazard { get; private set; }
    [field: SerializeField]
    public bool CreateOutsideHazardConfig { get; private set; } = true;

    [field: SerializeField]
    public List<NamespacedKeyWithAnimationCurve> OutsideMoonCurveSpawnWeights { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedKeyWithAnimationCurve> OutsideInteriorCurveSpawnWeights { get; private set; } = new();
    [field: SerializeField]
    public bool OutsidePrioritiseMoonConfig { get; private set; } = true;

    [field: SerializeField]
    public bool CreateOutsideCurveSpawnWeightsConfig { get; private set; } = true;

    public MapObjectConfig Config { get; private set; }

    public override void Register(DuskRegistrationContext registrationContext)
    {
        base.Register(registrationContext);
        using ConfigContext section = registrationContext.Mod.ConfigManager.CreateConfigSectionForBundleData(registrationContext.AssetBundleData);
        Config = CreateMapObjectConfig(section);
        BaseConfig = Config;

        DawnLib.DefineMapObject(TypedKey, GameObject, builder =>
        {
            if (Config.InsideHazard?.Value ?? IsInsideHazard)
            {
                MapObjectSpawnMechanics insideSpawnMechanics = new(
                    () => Config.InsideMoonCurveSpawnWeights?.Value ?? CurvesToConfigString(InsideMoonCurveSpawnWeights),
                    () => Config.InsideInteriorCurveSpawnWeights?.Value ?? CurvesToConfigString(InsideInteriorCurveSpawnWeights),
                    () => Config.InsidePrioritiseMoon?.Value ?? InsidePrioritiseMoonConfig);

                builder.DefineInside(insideBuilder =>
                {
                    insideBuilder.OverrideSpawnFacingWall(InsideMapObjectSettings.spawnFacingWall);
                    insideBuilder.OverrideSpawnFacingAwayFromWall(InsideMapObjectSettings.spawnFacingAwayFromWall);
                    insideBuilder.OverrideRequireDistanceBetweenSpawns(InsideMapObjectSettings.requireDistanceBetweenSpawns);
                    insideBuilder.OverrideDisallowSpawningNearEntrances(InsideMapObjectSettings.disallowSpawningNearEntrances);
                    insideBuilder.OverrideSpawnWithBackToWall(InsideMapObjectSettings.spawnWithBackToWall);
                    insideBuilder.OverrideSpawnWithBackFlushAgainstWall(InsideMapObjectSettings.spawnWithBackFlushAgainstWall);
                    insideBuilder.OverrideAllowInMineshaft(InsideMapObjectSettings.allowInMineshaft);
                    insideBuilder.SetWeights(weightProfile =>
                    {
                        weightProfile.AddSource(insideSpawnMechanics);
                    });
                });
            }

            if (Config.OutsideHazard?.Value ?? IsOutsideHazard)
            {
                MapObjectSpawnMechanics outsideSpawnMechanics = new(
                    () => Config.OutsideMoonCurveSpawnWeights?.Value ?? CurvesToConfigString(OutsideMoonCurveSpawnWeights),
                    () => Config.OutsideInteriorCurveSpawnWeights?.Value ?? CurvesToConfigString(OutsideInteriorCurveSpawnWeights),
                    () => Config.OutsidePrioritiseMoon?.Value ?? OutsidePrioritiseMoonConfig);

                builder.DefineOutside(outsideBuilder =>
                {
                    outsideBuilder.OverrideAlignWithTerrain(OutsideMapObjectSettings.alignWithTerrain);
                    outsideBuilder.OverrideMinimumNodeSpawnRequirement(OutsideMapObjectSettings.minimumAINodeSpawnRequirement);
                    outsideBuilder.OverrideObjectWidth(OutsideMapObjectSettings.objectWidth);
                    outsideBuilder.OverrideRotationOffset(OutsideMapObjectSettings.rotationOffset);
                    outsideBuilder.OverrideSpawnFacingAwayFromWall(OutsideMapObjectSettings.spawnFacingAwayFromWall);
                    outsideBuilder.OverrideSpawnableFloorTags(OutsideMapObjectSettings.spawnableFloorTags);
                    outsideBuilder.OverrideDestroyTrees(OutsideMapObjectSettings.destroyTrees);
                    outsideBuilder.SetWeights(weightProfile =>
                    {
                        weightProfile.AddSource(outsideSpawnMechanics);
                    });
                });
            }

            ApplyTagsTo(builder);
        });
    }

    public MapObjectConfig CreateMapObjectConfig(ConfigContext section)
    {
        MapObjectConfig mapObjectConfig = new(section, EntityNameReference);

        ConfigEntry<bool>? insideHazard = null, outsideHazard = null, insidePrioritiseMoon = null, outsidePrioritiseMoon = null;
        ConfigEntry<string>? insideMoonCurves = null, insideInteriorCurves = null, outsideMoonCurves = null, outsideInteriorCurves = null;
        if (CreateInsideHazardConfig)
        {
            insideHazard = section.Bind($"{EntityNameReference} | Is Inside Hazard", $"Whether {EntityNameReference} is an inside hazard", IsInsideHazard);
        }

        if (CreateOutsideHazardConfig)
        {
            outsideHazard = section.Bind($"{EntityNameReference} | Is Outside Hazard", $"Whether {EntityNameReference} is an outside hazard", IsOutsideHazard);
        }

        string insideMoonStringToUse = CurvesToConfigString(InsideMoonCurveSpawnWeights);
        string insideInteriorStringToUse = CurvesToConfigString(InsideInteriorCurveSpawnWeights);
        if ((insideHazard?.Value ?? IsInsideHazard) && CreateInsideCurveSpawnWeightsConfig)
        {
            insidePrioritiseMoon = section.Bind($"{EntityNameReference} | Inside Spawn Prioritise Moon", $"Whether {EntityNameReference} should prioritise moon curves rather than interior curves when spawning inside.", InsidePrioritiseMoonConfig);
            insideMoonCurves = section.Bind($"{EntityNameReference} | Inside Moon Spawn Weights", $"Curve weights for {EntityNameReference} when spawning inside using Moon weights.", insideMoonStringToUse);
            insideInteriorCurves = section.Bind($"{EntityNameReference} | Inside Interior Spawn Weights", $"Curve weights for {EntityNameReference} when spawning inside using Interior weights.", insideInteriorStringToUse);
        }

        string outsideMoonStringToUse = CurvesToConfigString(OutsideMoonCurveSpawnWeights);
        string outsideInteriorStringToUse = CurvesToConfigString(OutsideInteriorCurveSpawnWeights);
        if ((outsideHazard?.Value ?? IsOutsideHazard) && CreateOutsideCurveSpawnWeightsConfig)
        {
            outsidePrioritiseMoon = section.Bind($"{EntityNameReference} | Outside Spawn Prioritise Moon", $"Whether {EntityNameReference} should prioritise moon curves rather than interior curves when spawning outside.", OutsidePrioritiseMoonConfig);
            outsideMoonCurves = section.Bind($"{EntityNameReference} | Outside Moon Spawn Weights", $"Curve weights for {EntityNameReference} when spawning outside using Moon weights.", outsideMoonStringToUse);
            outsideInteriorCurves = section.Bind($"{EntityNameReference} | Outside Interior Spawn Weights", $"Curve weights for {EntityNameReference} when spawning outside using Interior weights.", outsideInteriorStringToUse);
        }

        mapObjectConfig.InsideHazard = insideHazard;
        mapObjectConfig.OutsideHazard = outsideHazard;

        mapObjectConfig.InsideMoonCurveSpawnWeights = insideMoonCurves;
        mapObjectConfig.InsideInteriorCurveSpawnWeights = insideInteriorCurves;
        mapObjectConfig.OutsideMoonCurveSpawnWeights = outsideMoonCurves;
        mapObjectConfig.OutsideInteriorCurveSpawnWeights = outsideInteriorCurves;

        mapObjectConfig.InsidePrioritiseMoon = insidePrioritiseMoon;
        mapObjectConfig.OutsidePrioritiseMoon = outsidePrioritiseMoon;

        if (!mapObjectConfig.UserAllowedToEdit())
        {
            DuskBaseConfig.AssignValueIfNotNull(mapObjectConfig.InsideHazard, IsInsideHazard);
            DuskBaseConfig.AssignValueIfNotNull(mapObjectConfig.OutsideHazard, IsOutsideHazard);

            DuskBaseConfig.AssignValueIfNotNull(mapObjectConfig.InsideMoonCurveSpawnWeights, insideMoonStringToUse);
            DuskBaseConfig.AssignValueIfNotNull(mapObjectConfig.InsideInteriorCurveSpawnWeights, insideInteriorStringToUse);
            DuskBaseConfig.AssignValueIfNotNull(mapObjectConfig.OutsideMoonCurveSpawnWeights, outsideMoonStringToUse);
            DuskBaseConfig.AssignValueIfNotNull(mapObjectConfig.OutsideInteriorCurveSpawnWeights, outsideInteriorStringToUse);

            DuskBaseConfig.AssignValueIfNotNull(mapObjectConfig.InsidePrioritiseMoon, InsidePrioritiseMoonConfig);
            DuskBaseConfig.AssignValueIfNotNull(mapObjectConfig.OutsidePrioritiseMoon, OutsidePrioritiseMoonConfig);
        }
        return mapObjectConfig;
    }

    public override void TryNetworkRegisterAssets()
    {
        if (!GameObject.TryGetComponent(out NetworkObject _))
            return;

        DawnLib.RegisterNetworkPrefab(GameObject);
    }

    protected override string EntityNameReference => MapObjectName;
}