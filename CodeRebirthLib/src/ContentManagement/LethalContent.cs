using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;
using DunGen;
using DunGen.Graph;
using Unity.Netcode;

namespace CodeRebirthLib.ContentManagement;
public static class LethalContent
{
    public static class Levels
    {
        private static readonly List<SelectableLevel> _allLevels = [], _vanillaLevels = [];

        public static IReadOnlyList<SelectableLevel> All => _allLevels.AsReadOnly();
        public static IReadOnlyList<SelectableLevel> Vanilla => _vanillaLevels.AsReadOnly();

        public static SelectableLevel CompanyBuildingLevel { get; private set; }
        public static SelectableLevel ExperimentationLevel { get; private set; }
        public static SelectableLevel MarchLevel { get; private set; }
        public static SelectableLevel VowLevel { get; private set; }
        public static SelectableLevel AssuranceLevel { get; private set; }
        public static SelectableLevel OffenseLevel { get; private set; }
        public static SelectableLevel RendLevel { get; private set; }
        public static SelectableLevel DineLevel { get; private set; }
        public static SelectableLevel TitanLevel { get; private set; }
        public static SelectableLevel AdamanceLevel { get; private set; }
        public static SelectableLevel ArtificeLevel { get; private set; }
        public static SelectableLevel EmbrionLevel { get; private set; }
        public static SelectableLevel LiquidationLevel { get; private set; }

        internal static void Init()
        {
            List<string> unknownTypes = [];

            var levels = StartOfRound.Instance.levels;
            foreach(SelectableLevel level in levels) {
                _allLevels.Add(level);
                CodeRebirthLibPlugin.ExtendedLogging($"Found level: {level.name}");

                PropertyInfo property = typeof(Levels).GetProperty(level.name);
                if (property == null) unknownTypes.Add(level.name);
                else {
                    property.SetValue(null, level);
                    _vanillaLevels.Add(level);
                }
            }

            CodeRebirthLibPlugin.ExtendedLogging($"Unknown levels: {string.Join(", ", unknownTypes)}");
        }
    }
    
    public static class Enemies
    {
        private static readonly List<EnemyType> _all = [], _vanilla = [];

        public static IReadOnlyList<EnemyType> All => _all.AsReadOnly();
        public static IReadOnlyList<EnemyType> Vanilla => _vanilla.AsReadOnly();   
        public static IEnumerable<CREnemyDefinition> CRLib => CRMod.AllMods.SelectMany(mod => mod.EnemyRegistry());
        
        public static EnemyType Flowerman { get; private set; }
        public static EnemyType Centipede { get; private set; }
        public static EnemyType MouthDog { get; private set; }
        public static EnemyType Crawler { get; private set; }
        public static EnemyType HoarderBug { get; private set; }
        public static EnemyType SandSpider { get; private set; }
        public static EnemyType Blob { get; private set; }
        public static EnemyType ForestGiant { get; private set; }
        public static EnemyType DressGirl { get; private set; }
        public static EnemyType SpringMan { get; private set; }
        public static EnemyType SandWorm { get; private set; }
        public static EnemyType Jester { get; private set; }
        public static EnemyType Puffer { get; private set; }
        public static EnemyType Doublewing { get; private set; }
        public static EnemyType DocileLocustBees { get; private set; }
        public static EnemyType RedLocustBees { get; private set; }
        public static EnemyType BaboonHawk { get; private set; }
        public static EnemyType Nutcracker { get; private set; }
        public static EnemyType MaskedPlayerEnemy { get; private set; }
        public static EnemyType RadMech { get; private set; }
        public static EnemyType Butler { get; private set; }
        public static EnemyType ButlerBees { get; private set; }
        public static EnemyType FlowerSnake { get; private set; }
        public static EnemyType BushWolf { get; private set; }
        public static EnemyType ClaySurgeon { get; private set; }
        public static EnemyType CaveDweller { get; private set; }
        public static EnemyType GiantKiwi { get; private set; }

        internal static void Init()
        {
            List<string> unknownTypes = [];
            
            for (int i = 0; i < NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs.Count; i++)
            {
                NetworkPrefab networkPrefab = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[i];
                if (!networkPrefab.Prefab.TryGetComponent(out EnemyAI enemyAI) || enemyAI.enemyType == null)
                    continue;

                _all.Add(enemyAI.enemyType);
                CodeRebirthLibPlugin.ExtendedLogging($"Found enemy: {enemyAI.enemyType.name}");

                PropertyInfo property = typeof(Enemies).GetProperty(enemyAI.enemyType.name);
                if (property == null) unknownTypes.Add(enemyAI.enemyType.name);
                else
                {
                    property.SetValue(null, enemyAI.enemyType);
                    _vanilla.Add(enemyAI.enemyType);
                }
            }
        }
    }
    
    public static class Items
    {
        private static readonly List<Item> _all = [], _vanilla = [];

        public static IReadOnlyList<Item> All => _all.AsReadOnly();
        public static IReadOnlyList<Item> Vanilla => _vanilla.AsReadOnly();   
        public static IEnumerable<CRItemDefinition> CRLib => CRMod.AllMods.SelectMany(mod => mod.ItemRegistry());
        
        public static Item SevenBall { get; private set; }
        public static Item Airhorn { get; private set; }
        public static Item Bell { get; private set; }
        public static Item BigBolt { get; private set; }
        public static Item CashRegister { get; private set; }
        // todo: add all items or look at automatically generating this somehow so xu doesn't go insasne
        
        internal static void Init()
        {
            List<string> unknownTypes = [];
            
            for (int i = 0; i < NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs.Count; i++)
            {
                NetworkPrefab networkPrefab = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[i];
                if (!networkPrefab.Prefab.TryGetComponent(out GrabbableObject grabbable) || grabbable.itemProperties == null)
                    continue;

                _all.Add(grabbable.itemProperties);
                CodeRebirthLibPlugin.ExtendedLogging($"Found enemy: {grabbable.itemProperties.name}");

                PropertyInfo property = typeof(Items).GetProperty(grabbable.itemProperties.name.Replace("7", "Seven")); // todo: make this better
                if (property == null) unknownTypes.Add(grabbable.itemProperties.name);
                else
                {
                    property.SetValue(null, grabbable.itemProperties);
                    _vanilla.Add(grabbable.itemProperties);
                }
            }
            
            CodeRebirthLibPlugin.ExtendedLogging($"Unknown enemy types: {string.Join(", ", unknownTypes)}");
        }
    }

    public static class Dungeons
    {
        private static readonly List<DungeonFlow> _allFlows = [], _vanillaFlows = [];
        public static IReadOnlyList<DungeonFlow> AllFlows = _allFlows.AsReadOnly();
        public static IReadOnlyList<DungeonFlow> VanillaFlows = _vanillaFlows.AsReadOnly();
        
        public static DungeonFlow Level1Flow { get; private set; }
        public static DungeonFlow Level1Flow3Exits { get; private set; } // todo: does this get replaced by LLL?
        public static DungeonFlow Level1FlowExtraLarge { get; private set; }
        public static DungeonFlow Level2Flow { get; private set; }
        public static DungeonFlow Level3Flow { get; private set; }

        private static readonly List<DoorwaySocket> _vanillaSockets = [];
        public static IReadOnlyList<DoorwaySocket> VanillaDoorSockets = _vanillaSockets.AsReadOnly();
        
        internal static void Init()
        {
            List<string> unknownTypes = [];
            
            foreach (DungeonFlow flow in new List<DungeonFlow>()) // todo
            {
                _allFlows.Add(flow);
                CodeRebirthLibPlugin.ExtendedLogging($"Found dungeon flow: {flow.name}");

                PropertyInfo property = typeof(Dungeons).GetProperty(flow.name);
                if (property == null) unknownTypes.Add(flow.name);
                else
                {
                    property.SetValue(null, flow);
                    _vanillaFlows.Add(flow);
                }
            }

            foreach (DungeonFlow flow in VanillaFlows)
            {
                foreach (GameObjectChance chance in flow.GetUsedTileSets().Select(it => it.TileWeights.Weights).SelectMany(it => it))
                {
                    Doorway[] doorways = chance.Value.GetComponentsInChildren<Doorway>();

                    foreach (Doorway doorway in doorways)
                    {
                        if(_vanillaSockets.Contains(doorway.socket)) continue;
                        _vanillaSockets.Add(doorway.socket);
                    }
                }
            }
            
            CodeRebirthLibPlugin.ExtendedLogging($"Unknown enemy types: {string.Join(", ", unknownTypes)}");
        }
    }
}