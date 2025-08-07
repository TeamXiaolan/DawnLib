using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;
using DunGen;
using DunGen.Graph;
using LethalLevelLoader;
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
                if (property == null)
                {
                    unknownTypes.Add(enemyAI.enemyType.name);
                }
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
        public static Item BabyKiwiEgg { get; private set; }
        public static Item Bell { get; private set; }
        public static Item BeltBag { get; private set; }
        public static Item BigBolt { get; private set; }
        public static Item Binoculars { get; private set; }
        public static Item Boombox { get; private set; }
        public static Item BottleBin { get; private set; }
        public static Item Brush { get; private set; }
        public static Item Candy { get; private set; }
        public static Item CardboardBox { get; private set; }
        public static Item CashRegister { get; private set; }
        public static Item CaveDwellerBaby { get; private set; }
        public static Item ChemicalJug { get; private set; }
        public static Item Clipboard { get; private set; }
        public static Item Clock { get; private set; }
        public static Item ClownHorn { get; private set; }
        public static Item Cog1 { get; private set; }
        public static Item ComedyMask { get; private set; }
        public static Item ControlPad { get; private set; }
        public static Item Dentures { get; private set; }
        public static Item DiyFlashBang { get; private set; }
        public static Item DustPan { get; private set; }
        public static Item EasterEgg { get; private set; }
        public static Item EggBeater { get; private set; }
        public static Item EnginePart1 { get; private set; }
        public static Item ExtensionLadder { get; private set; }
        public static Item FancyLamp { get; private set; }
        public static Item FancyPainting { get; private set; }
        public static Item FishTestProp { get; private set; }
        public static Item FlashLaserPointer { get; private set; }
        public static Item Flashlight { get; private set; }
        public static Item Flask { get; private set; }
        public static Item GarbageLid { get; private set; }
        public static Item GiftBox { get; private set; }
        public static Item GoldBar { get; private set; }
        public static Item GunAmmo { get; private set; }
        public static Item Hairdryer { get; private set; }
        public static Item Jetpack { get; private set; }
        public static Item Key { get; private set; }
        public static Item Knife { get; private set; }
        public static Item LockPicker { get; private set; }
        public static Item LungApparatus { get; private set; }
        public static Item MagnifyGlass { get; private set; }
        public static Item MapDevice { get; private set; }
        public static Item MetalSheet { get; private set; }
        public static Item MoldPan { get; private set; }
        public static Item Mug { get; private set; }
        public static Item PerfumeBottle { get; private set; }
        public static Item Phone { get; private set; }
        public static Item PickleJar { get; private set; }
        public static Item PillBottle { get; private set; }
        public static Item PlasticCup { get; private set; }
        public static Item ProFlashlight { get; private set; }
        public static Item RadarBooster { get; private set; }
        public static Item Ragdoll { get; private set; }
        public static Item RedLocustHive { get; private set; }
        public static Item Remote { get; private set; }
        public static Item Ring { get; private set; }
        public static Item RobotToy { get; private set; }
        public static Item RubberDuck { get; private set; }
        public static Item Shotgun { get; private set; }
        public static Item Shovel { get; private set; }
        public static Item SoccerBall { get; private set; }
        public static Item SodaCanRed { get; private set; }
        public static Item SprayPaint { get; private set; }
        public static Item SteeringWheel { get; private set; }
        public static Item StickyNote { get; private set; }
        public static Item StopSign { get; private set; }
        public static Item StunGrenade { get; private set; }
        public static Item TeaKettle { get; private set; }
        public static Item ToiletPaperRolls { get; private set; }
        public static Item Toothpaste { get; private set; }
        public static Item ToyCube { get; private set; }
        public static Item ToyTrain { get; private set; }
        public static Item TragedyMask { get; private set; }
        public static Item TZPInhalant { get; private set; }
        public static Item WalkieTalkie { get; private set; }
        public static Item WeedKillerBottle { get; private set; }
        public static Item WhoopieCushion { get; private set; }
        public static Item YieldSign { get; private set; }
        public static Item ZapGun { get; private set; }
        public static Item Zeddog { get; private set; }

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
                CodeRebirthLibPlugin.ExtendedLogging($"Found item: {grabbable.itemProperties.name}");

                PropertyInfo property = typeof(Items).GetProperty(grabbable.itemProperties.name.Replace("7", "Seven")); // todo: make this better
                if (property == null) unknownTypes.Add(grabbable.itemProperties.name);
                else
                {
                    property.SetValue(null, grabbable.itemProperties);
                    _vanilla.Add(grabbable.itemProperties);
                }
            }

            CodeRebirthLibPlugin.ExtendedLogging($"Unknown item types: {string.Join(", ", unknownTypes)}");
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
            
            foreach (DungeonFlow flow in RoundManager.Instance.dungeonFlowTypes.Select(i => i.dungeonFlow))
            {
                _allFlows.Add(flow);
                CodeRebirthLibPlugin.ExtendedLogging($"Found dungeon flow: {flow.name}");

                PropertyInfo property = typeof(Dungeons).GetProperty(flow.name);
                if (property == null)
                {
                    unknownTypes.Add(flow.name);
                }
                else
                {
                    property.SetValue(null, flow);
                    _vanillaFlows.Add(flow);
                }
            }

            CodeRebirthLibPlugin.ExtendedLogging($"Unknown dungeon flows: {string.Join(", ", unknownTypes)}");

            foreach (DungeonFlow flow in VanillaFlows)
            {
                foreach (GameObjectChance chance in flow.GetUsedTileSets().Select(it => it.TileWeights.Weights).SelectMany(it => it))
                {
                    Doorway[] doorways = chance.Value.GetComponentsInChildren<Doorway>();

                    foreach (Doorway doorway in doorways)
                    {
                        if (_vanillaSockets.Contains(doorway.socket))
                            continue;

                        _vanillaSockets.Add(doorway.socket);
                    }
                }
            }            
        }
    }
}