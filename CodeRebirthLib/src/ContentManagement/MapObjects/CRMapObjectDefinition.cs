using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Exceptions;
using CodeRebirthLib.Patches;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.MapObjects;
[CreateAssetMenu(fileName = "New Map Definition", menuName = "CodeRebirthLib/Definitions/Map Definition")]
public class CRMapObjectDefinition : CRContentDefinition<MapObjectData>
{
    public const string REGISTRY_ID = "map_objects";

    [field: FormerlySerializedAs("gameObject")] [field: SerializeField]
    public GameObject GameObject { get; private set; }

    [field: FormerlySerializedAs("objectName")] [field: FormerlySerializedAs("ObjectName")] [field: SerializeField]
    public string MapObjectName { get; private set; }

    [field: FormerlySerializedAs("alignWithTerrain")] [field: SerializeField]
    public bool AlignWithTerrain { get; private set; }

    [field: SerializeField]
    public SpawnableMapObject InsideMapObjectSettings { get; private set; } = new();
    
    public MapObjectConfig Config { get; private set; }
    public MapObjectSpawnMechanics? InsideSpawnMechanics { get; private set; }
    public MapObjectSpawnMechanics? OutsideSpawnMechanics { get; private set; }

    public bool HasNetworkObject { get; private set; }

    protected override string EntityNameReference => MapObjectName;

    public override void Register(CRMod mod, MapObjectData data)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateMapObjectConfig(section, data, EntityNameReference);

        HasNetworkObject = GameObject.GetComponent<NetworkObject>();
        
        if (Config.InsideHazard?.Value ?? data.isInsideHazard)
        {
            try
            {
                InsideSpawnMechanics = new MapObjectSpawnMechanics(Config.InsideCurveSpawnWeights?.Value ?? data.defaultInsideCurveSpawnWeights);
            }
            catch (MalformedAnimationCurveConfigException exception)
            {
                mod.Logger?.LogError($"Failed to parse inside curve for map object: {EntityNameReference}");
                exception.LogNicely(mod.Logger);
            }
        }

        if (Config.OutsideHazard?.Value ?? data.isOutsideHazard)
        {
            try
            {
                OutsideSpawnMechanics = new MapObjectSpawnMechanics(Config.OutsideCurveSpawnWeights?.Value ?? data.defaultOutsideCurveSpawnWeights);
            }
            catch (MalformedAnimationCurveConfigException exception)
            {
                mod.Logger?.LogError($"Failed to parse outside curve for map object: {EntityNameReference}");
                exception.LogNicely(mod.Logger);
            }
        }

        mod.MapObjectRegistry().Register(this);
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

    public static void RegisterTo(CRMod mod)
    {
        mod.CreateRegistry(REGISTRY_ID, new CRRegistry<CRMapObjectDefinition>());
    }

    public override List<MapObjectData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.mapObjects).ToList();
    }
}