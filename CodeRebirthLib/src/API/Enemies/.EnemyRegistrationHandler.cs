using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CodeRebirthLib;

static class EnemyRegistrationHandler
{
    static NamespacedKey<CRMoonInfo> ToNamespacedKey(this SelectableLevel level)
    {
        return NamespacedKey<CRMoonInfo>.Vanilla(new string(level.PlanetName.SkipWhile(c => !char.IsLetter(c)).ToArray()).Replace(" ", "_").ToLower(CultureInfo.InvariantCulture));
    }
    
    internal static void Init()
    {
        On.StartOfRound.Awake += CollectLevels;
        On.StartOfRound.Awake += RegisterEnemies;
    }

    private static void CollectLevels(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.Moons.IsFrozen)
        {
            orig(self);
            return;
        }

        foreach (SelectableLevel level in self.levels)
        {
            NamespacedKey<CRMoonInfo> key = level.ToNamespacedKey();
            CRMoonInfo moonInfo = new CRMoonInfo(key, level);
            LethalContent.Moons.Register(moonInfo);
        }
        
        LethalContent.Moons.Freeze();
        orig(self);
    }
    
    private static void RegisterEnemies(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.Enemies.IsFrozen)
        {
            orig(self);
            return;
        }
        
        foreach (SelectableLevel level in self.levels)
        {
            NamespacedKey<CRMoonInfo> moonKey = level.ToNamespacedKey();
            
            List<SpawnableEnemyWithRarity> levelEnemies = new List<SpawnableEnemyWithRarity>();
            levelEnemies.AddRange(level.Enemies);
            levelEnemies.AddRange(level.OutsideEnemies);
            levelEnemies.AddRange(level.DaytimeEnemies);
            foreach (SpawnableEnemyWithRarity enemy in levelEnemies)
            {
                NamespacedKey<CREnemyInfo>? key = (NamespacedKey<CREnemyInfo>?) typeof(EnemyKeys).GetField(enemy.enemyType.enemyName.Replace(" ", ""))?.GetValue(null);
                if(key == null)
                    continue;
                
                if(LethalContent.Enemies.ContainsKey(key))
                    continue;
                
                // todo: do weight calculation stuff
                CREnemyInfo enemyInfo = new CREnemyInfo(key, enemy.enemyType, null, null, null);
                LethalContent.Enemies.Register(enemyInfo);
            }
            
            foreach (CREnemyInfo enemyInfo in LethalContent.Enemies.Values)
            {
                if (enemyInfo.Key.IsVanilla())
                    continue; // also ensure not to register vanilla stuff again

                TryAddToEnemyList(enemyInfo, enemyInfo.OutsideWeights, moonKey, level.OutsideEnemies);
                TryAddToEnemyList(enemyInfo, enemyInfo.DaytimeWeights, moonKey, level.DaytimeEnemies);
                TryAddToEnemyList(enemyInfo, enemyInfo.InsideWeights, moonKey, level.Enemies);
            }
        }
        
        LethalContent.Enemies.Freeze();
        orig(self);
    }
    private static void TryAddToEnemyList(CREnemyInfo enemyInfo, WeightTable<CRMoonInfo>? weights, NamespacedKey<CRMoonInfo> moonKey, List<SpawnableEnemyWithRarity> list)
    {
        if (weights == null) return;
        
        SpawnableEnemyWithRarity spawnDef = new SpawnableEnemyWithRarity()
        {
            enemyType = enemyInfo.Enemy,
            rarity = 0 // todo: dynamic update
        };
        list.Add(spawnDef);
        
        // todo: keep track of spawnDef to later update the rarity.
    }
}