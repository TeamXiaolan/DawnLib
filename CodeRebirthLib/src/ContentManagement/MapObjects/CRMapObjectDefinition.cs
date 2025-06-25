using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Exceptions;
using CodeRebirthLib.Patches;
using LethalLib.Extras;
using LethalLib.Modules;
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

    [field: FormerlySerializedAs("objectName")] [field: SerializeField]
    public string ObjectName { get; private set; }

    [field: FormerlySerializedAs("alignWithTerrain")] [field: SerializeField]
    public bool AlignWithTerrain { get; private set; }

    public MapObjectConfig Config { get; private set; }
    public MapObjectSpawnMechanics? InsideSpawnMechanics { get; private set; }
    public MapObjectSpawnMechanics? OutsideSpawnMechanics { get; private set; }

    public override void Register(CRMod mod, MapObjectData data)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateMapObjectConfig(mod, section, data, ObjectName);

        if (Config.InsideHazard?.Value ?? data.isInsideHazard)
        {
            SpawnableMapObjectDef insideDef = CreateInstance<SpawnableMapObjectDef>();
            insideDef.spawnableMapObject = new SpawnableMapObject
            {
                prefabToSpawn = GameObject,
            };
            try
            {
                InsideSpawnMechanics = new MapObjectSpawnMechanics(Config.InsideCurveSpawnWeights?.Value ?? data.defaultInsideCurveSpawnWeights);
            }
            catch (MalformedAnimationCurveConfigException exception)
            {
                mod.Logger?.LogError($"Failed to parse outside curve for map object: {ObjectName}");
                exception.LogNicely(mod.Logger);
                return; // shouldn't probably be a return
            }

            LethalLib.Modules.MapObjects.RegisterMapObject(
                insideDef,
                Levels.LevelTypes.All,
                InsideSpawnMechanics.LevelOverrides,
                InsideSpawnMechanics.CurveFunction
            );
        }

        if (Config.OutsideHazard?.Value ?? data.isOutsideHazard)
        {
            SpawnableOutsideObjectDef outsideDef = CreateInstance<SpawnableOutsideObjectDef>();
            outsideDef.spawnableMapObject = new SpawnableOutsideObjectWithRarity
            {
                spawnableObject = CreateInstance<SpawnableOutsideObject>(),
            };
            outsideDef.spawnableMapObject.spawnableObject.prefabToSpawn = GameObject;

            try
            {
                OutsideSpawnMechanics = new MapObjectSpawnMechanics(Config.OutsideCurveSpawnWeights?.Value ?? data.defaultOutsideCurveSpawnWeights);
            }
            catch (MalformedAnimationCurveConfigException exception)
            {
                mod.Logger?.LogError($"Failed to parse outside curve for map object: {ObjectName}");
                exception.LogNicely(mod.Logger);
                return; // shouldn't probably be a return
            }

            RegisteredCRMapObject registeredCRMapObject = new()
            {
                alignWithTerrain = AlignWithTerrain,
                hasNetworkObject = outsideDef.spawnableMapObject.spawnableObject.prefabToSpawn.GetComponent<NetworkObject>() != null,
                outsideObject = outsideDef.spawnableMapObject,
                levels = Levels.LevelTypes.All,
                spawnLevelOverrides = OutsideSpawnMechanics.LevelOverrides,
                spawnRateFunction = OutsideSpawnMechanics.CurveFunction,
            };
            RoundManagerPatch.registeredMapObjects.Add(registeredCRMapObject);
        }

        mod.MapObjectRegistry().Register(this);
    }

    public static MapObjectConfig CreateMapObjectConfig(CRMod mod, ConfigContext section, MapObjectData data, string objectName)
    {
        ConfigEntry<bool>? insideHazard = null, outsideHazard = null;
        ConfigEntry<string>? insideCurves = null, outsideCurves = null;
        if (data.createInsideHazardConfig)
            insideHazard = section.Bind("Is Inside Hazard", $"Whether {objectName} is an inside hazard", data.isInsideHazard);

        if (data.createOutsideHazardConfig)
            outsideHazard = section.Bind("Is Outside Hazard", $"Whether {objectName} is an outside hazard", data.isOutsideHazard);

        if ((insideHazard?.Value ?? false) && data.createInsideCurveSpawnWeightsConfig)
            insideCurves = section.Bind("Inside Spawn Weights", $"Curve weights for {objectName} when spawning inside.", data.defaultInsideCurveSpawnWeights);

        if ((outsideHazard?.Value ?? false) && data.createOutsideCurveSpawnWeightsConfig)
            outsideCurves = section.Bind("Outside Spawn Weights", $"Curve weights for {objectName} when spawning outside.", data.defaultOutsideCurveSpawnWeights);

        return new MapObjectConfig
        {
            InsideHazard = insideHazard, OutsideHazard = outsideHazard,
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