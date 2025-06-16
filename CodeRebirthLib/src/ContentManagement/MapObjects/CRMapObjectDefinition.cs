using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ContentManagement.Items;
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
    [field: FormerlySerializedAs("gameObject"), SerializeField]
    public GameObject GameObject { get; private set; }
    [field: FormerlySerializedAs("objectName"), SerializeField]
    public string ObjectName { get; private set; }
    [field: FormerlySerializedAs("alignWithTerrain"), SerializeField]
    public bool AlignWithTerrain { get; private set; }
    
    // xu what is this.
    // [field: SerializeField]
    // public SpawnSyncedCRObject.CRObjectType CRObjectType { get; private set; }
    
    public MapObjectConfig Config { get; private set; }
    public MapObjectSpawnMechanics? InsideSpawnMechanics { get; private set; }
    public MapObjectSpawnMechanics? OutsideSpawnMechanics { get; private set; }
    
    public override void Register(CRMod mod, MapObjectData data)
    {
        Config = CreateMapObjectConfig(mod, data, ObjectName);

        
        
        if (Config.InsideHazard.Value)
        {
            SpawnableMapObjectDef insideDef = ScriptableObject.CreateInstance<SpawnableMapObjectDef>();
            insideDef.spawnableMapObject = new SpawnableMapObject
            {
                prefabToSpawn = GameObject
            };
            try
            {
                InsideSpawnMechanics = new MapObjectSpawnMechanics(Config.InsideCurveSpawnWeights!.Value);
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

        if (Config.OutsideHazard.Value)
        {
            SpawnableOutsideObjectDef outsideDef = ScriptableObject.CreateInstance<SpawnableOutsideObjectDef>();
            outsideDef.spawnableMapObject = new SpawnableOutsideObjectWithRarity
            {
                spawnableObject = ScriptableObject.CreateInstance<SpawnableOutsideObject>()
            };
            outsideDef.spawnableMapObject.spawnableObject.prefabToSpawn = GameObject;
            
            try
            {
                OutsideSpawnMechanics = new MapObjectSpawnMechanics(Config.OutsideCurveSpawnWeights!.Value);
            }
            catch (MalformedAnimationCurveConfigException exception)
            {
                mod.Logger?.LogError($"Failed to parse outside curve for map object: {ObjectName}");
                exception.LogNicely(mod.Logger);
                return; // shouldn't probably be a return
            }

            RegisteredCRMapObject registeredCRMapObject = new RegisteredCRMapObject()
            {
                alignWithTerrain = AlignWithTerrain,
                hasNetworkObject = outsideDef.spawnableMapObject.spawnableObject.prefabToSpawn.GetComponent<NetworkObject>() != null,
                outsideObject = outsideDef.spawnableMapObject,
                levels = Levels.LevelTypes.All,
                spawnLevelOverrides = OutsideSpawnMechanics.LevelOverrides,
                spawnRateFunction = OutsideSpawnMechanics.CurveFunction
            };
            RoundManagerPatch.registeredMapObjects.Add(registeredCRMapObject);
        }
        
        mod.MapObjectRegistry().Register(this);
    }

    public static MapObjectConfig CreateMapObjectConfig(CRMod mod, MapObjectData data, string objectName)
    {
        using(ConfigContext section = mod.ConfigManager.CreateConfigSection(objectName))
        {
            ConfigEntry<bool> insideHazard = section.Bind("Is Inside Hazard", $"Whether {objectName} is an inside hazard", data.isInsideHazard);
            ConfigEntry<bool> outsideHazard = section.Bind("Is Outside Hazard", $"Whether {objectName} is an outside hazard", data.isOutsideHazard);

            return new MapObjectConfig
            {
                InsideHazard = insideHazard, OutsideHazard = outsideHazard,
                InsideCurveSpawnWeights = section.Bind("Inside Spawn Weights", $"Curve weights for {objectName} when spawning inside.", data.defaultInsideCurveSpawnWeights),
                OutsideCurveSpawnWeights = section.Bind("Outside Spawn Weights", $"Curve weights for {objectName} when spawning outside.", data.defaultOutsideCurveSpawnWeights)
            };
        }
    }
    
    public const string REGISTRY_ID = "map_objects";

    public static void RegisterTo(CRMod mod)
    {
        mod.CreateRegistry(REGISTRY_ID, new CRRegistry<CRMapObjectDefinition>());
    }
    
    public override List<MapObjectData> GetEntities(CRMod mod) => mod.Content.assetBundles.SelectMany(it => it.mapObjects).ToList();
}