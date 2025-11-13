using System;
using System.Collections.Generic;
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

    [field: Header("Obsolete")]
    [field: Obsolete]
    [field: SerializeField]
    public string DefaultOutsideCurveSpawnWeights { get; private set; }
    [field: SerializeField]
    [field: Obsolete]
    public string DefaultInsideCurveSpawnWeights { get; private set; }

    public MapObjectConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateMapObjectConfig(section);

        DawnLib.DefineMapObject(TypedKey, GameObject, builder =>
        {
            if (Config.InsideHazard?.Value ?? IsInsideHazard)
            {
                string moonStringToUse = Config.InsideMoonCurveSpawnWeights?.Value ?? DefaultInsideCurveSpawnWeights;
                if (string.IsNullOrEmpty(moonStringToUse))
                {
                    foreach (NamespacedKeyWithAnimationCurve curve in InsideMoonCurveSpawnWeights)
                    {
                        moonStringToUse += $"{curve.Key} - {ConfigManager.ParseString(curve.Curve)} | ";
                    }
                    if (!string.IsNullOrEmpty(moonStringToUse))
                    {
                        moonStringToUse = moonStringToUse[..^3];
                    }
                }
                string interiorStringToUse = Config.InsideInteriorCurveSpawnWeights?.Value ?? string.Empty;
                if (string.IsNullOrEmpty(interiorStringToUse))
                {
                    foreach (NamespacedKeyWithAnimationCurve curve in InsideInteriorCurveSpawnWeights)
                    {
                        interiorStringToUse += $"{curve.Key} - {ConfigManager.ParseString(curve.Curve)} | ";
                    }
                    if (!string.IsNullOrEmpty(interiorStringToUse))
                    {
                        interiorStringToUse = interiorStringToUse[..^3];
                    }
                }
                MapObjectSpawnMechanics InsideSpawnMechanics = new(moonStringToUse, interiorStringToUse, Config.InsidePrioritiseMoon?.Value ?? InsidePrioritiseMoonConfig);
                builder.DefineInside(insideBuilder =>
                {
                    insideBuilder.OverrideSpawnFacingWall(InsideMapObjectSettings.spawnFacingWall);
                    insideBuilder.OverrideSpawnFacingAwayFromWall(InsideMapObjectSettings.spawnFacingAwayFromWall);
                    insideBuilder.OverrideRequireDistanceBetweenSpawns(InsideMapObjectSettings.requireDistanceBetweenSpawns);
                    insideBuilder.OverrideDisallowSpawningNearEntrances(InsideMapObjectSettings.disallowSpawningNearEntrances);
                    insideBuilder.OverrideSpawnWithBackToWall(InsideMapObjectSettings.spawnWithBackToWall);
                    insideBuilder.OverrideSpawnWithBackFlushAgainstWall(InsideMapObjectSettings.spawnWithBackFlushAgainstWall);
                    insideBuilder.SetWeights(weightBuilder =>
                    {
                        weightBuilder.SetGlobalCurve(InsideSpawnMechanics);
                    });
                });
            }

            if (Config.OutsideHazard?.Value ?? IsOutsideHazard)
            {
                string moonStringToUse = Config.OutsideMoonCurveSpawnWeights?.Value ?? DefaultOutsideCurveSpawnWeights;
                if (string.IsNullOrEmpty(moonStringToUse))
                {
                    foreach (NamespacedKeyWithAnimationCurve curve in OutsideMoonCurveSpawnWeights)
                    {
                        moonStringToUse += $"{curve.Key} - {ConfigManager.ParseString(curve.Curve)} | ";
                    }
                    if (string.IsNullOrEmpty(moonStringToUse))
                    {
                        moonStringToUse = moonStringToUse[..^3];
                    }
                }
                string interiorStringToUse = Config.OutsideInteriorCurveSpawnWeights?.Value ?? string.Empty;
                if (string.IsNullOrEmpty(interiorStringToUse))
                {
                    foreach (NamespacedKeyWithAnimationCurve curve in OutsideInteriorCurveSpawnWeights)
                    {
                        interiorStringToUse += $"{curve.Key} - {ConfigManager.ParseString(curve.Curve)} | ";
                    }
                    if (!string.IsNullOrEmpty(interiorStringToUse))
                    {
                        interiorStringToUse = interiorStringToUse[..^3];
                    }
                }
                MapObjectSpawnMechanics OutsideSpawnMechanics = new(moonStringToUse, interiorStringToUse, Config.OutsidePrioritiseMoon?.Value ?? OutsidePrioritiseMoonConfig);
                builder.DefineOutside(outsideBuilder =>
                {
                    outsideBuilder.OverrideAlignWithTerrain(OutsideMapObjectSettings.AlignWithTerrain);
                    outsideBuilder.SetWeights(weightBuilder =>
                    {
                        weightBuilder.SetGlobalCurve(OutsideSpawnMechanics);
                    });
                });
            }

            ApplyTagsTo(builder);
        });
    }

    public MapObjectConfig CreateMapObjectConfig(ConfigContext section)
    {
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

        if ((insideHazard?.Value ?? IsInsideHazard) && CreateInsideCurveSpawnWeightsConfig)
        {
            insidePrioritiseMoon = section.Bind($"{EntityNameReference} | Inside Spawn Prioritise Moon", $"Whether {EntityNameReference} should prioritise moon curves rather than interior curves when spawning inside.", InsidePrioritiseMoonConfig);
            string moonStringToUse = Config.InsideMoonCurveSpawnWeights?.Value ?? DefaultInsideCurveSpawnWeights;
            if (string.IsNullOrEmpty(moonStringToUse))
            {
                foreach (NamespacedKeyWithAnimationCurve curve in InsideMoonCurveSpawnWeights)
                {
                    moonStringToUse += $"{curve.Key} - {ConfigManager.ParseString(curve.Curve)} | ";
                }
                if (!string.IsNullOrEmpty(moonStringToUse))
                {
                    moonStringToUse = moonStringToUse[..^3];
                }
            }
            string interiorStringToUse = Config.InsideInteriorCurveSpawnWeights?.Value ?? string.Empty;
            if (string.IsNullOrEmpty(interiorStringToUse))
            {
                foreach (NamespacedKeyWithAnimationCurve curve in InsideInteriorCurveSpawnWeights)
                {
                    interiorStringToUse += $"{curve.Key} - {ConfigManager.ParseString(curve.Curve)} | ";
                }
                if (!string.IsNullOrEmpty(interiorStringToUse))
                {
                    interiorStringToUse = interiorStringToUse[..^3];
                }
            }

            insideMoonCurves = section.Bind($"{EntityNameReference} | Inside Moon Spawn Weights", $"Curve weights for {EntityNameReference} when spawning inside using Moon weights.", moonStringToUse);
            insideInteriorCurves = section.Bind($"{EntityNameReference} | Inside Interior Spawn Weights", $"Curve weights for {EntityNameReference} when spawning inside using Interior weights.", interiorStringToUse);
        }

        if ((outsideHazard?.Value ?? IsOutsideHazard) && CreateOutsideCurveSpawnWeightsConfig)
        {
            outsidePrioritiseMoon = section.Bind($"{EntityNameReference} | Outside Spawn Prioritise Moon", $"Whether {EntityNameReference} should prioritise moon curves rather than interior curves when spawning outside.", OutsidePrioritiseMoonConfig);
            string moonStringToUse = Config.OutsideMoonCurveSpawnWeights?.Value ?? DefaultOutsideCurveSpawnWeights;
            if (string.IsNullOrEmpty(moonStringToUse))
            {
                foreach (NamespacedKeyWithAnimationCurve curve in OutsideMoonCurveSpawnWeights)
                {
                    moonStringToUse += $"{curve.Key} - {ConfigManager.ParseString(curve.Curve)} | ";
                }
                if (string.IsNullOrEmpty(moonStringToUse))
                {
                    moonStringToUse = moonStringToUse[..^3];
                }
            }
            string interiorStringToUse = Config.OutsideInteriorCurveSpawnWeights?.Value ?? string.Empty;
            if (string.IsNullOrEmpty(interiorStringToUse))
            {
                foreach (NamespacedKeyWithAnimationCurve curve in OutsideInteriorCurveSpawnWeights)
                {
                    interiorStringToUse += $"{curve.Key} - {ConfigManager.ParseString(curve.Curve)} | ";
                }
                if (!string.IsNullOrEmpty(interiorStringToUse))
                {
                    interiorStringToUse = interiorStringToUse[..^3];
                }
            }
            outsideMoonCurves = section.Bind($"{EntityNameReference} | Outside Moon Spawn Weights", $"Curve weights for {EntityNameReference} when spawning outside using Moon weights.", moonStringToUse);
            outsideInteriorCurves = section.Bind($"{EntityNameReference} | Outside Interior Spawn Weights", $"Curve weights for {EntityNameReference} when spawning outside using Interior weights.", interiorStringToUse);
        }

        return new MapObjectConfig
        {
            InsideHazard = insideHazard,
            OutsideHazard = outsideHazard,
            InsideMoonCurveSpawnWeights = insideMoonCurves,
            InsideInteriorCurveSpawnWeights = insideInteriorCurves,
            OutsideMoonCurveSpawnWeights = outsideMoonCurves,
            OutsideInteriorCurveSpawnWeights = outsideInteriorCurves,
            InsidePrioritiseMoon = insidePrioritiseMoon,
            OutsidePrioritiseMoon = outsidePrioritiseMoon
        };
    }

    protected override string EntityNameReference => MapObjectName;
}