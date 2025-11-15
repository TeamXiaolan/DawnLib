using System;
using System.Collections.Generic;
using DunGen;
using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

namespace Dawn.Internal;

static class MiscFixesPatch
{
    internal static List<GameObject> networkPrefabsToAdd = new();
    internal static List<GameObject> soundPrefabsToFix = new();
    internal static List<GameObject> tilesToFixSockets = new();

    internal static void Init()
    {
        On.GameNetworkManager.Start += AddNetworkPrefabToNetworkConfig;
        On.MenuManager.Start += DoSoundFixes;
        // TODO replace these changes with prefab changes to get rid of the fake SO's once and for all
        On.ButlerEnemyAI.Start += FixButlerBlankReferences;
        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(EnemyAINestSpawnObject), "Awake"), FixNestBlankReferences));
        On.GiantKiwiAI.Start += FixKiwiBlankReferences;
        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(HauntedMaskItem), "Awake"), FixHauntedMaskBlankReferences));
        On.LungProp.Start += FixLungPropBlankReferences;
        // TODO end
        LethalContent.Dungeons.OnFreeze += FixTileSetSockets;
        LethalContent.Items.OnFreeze += FixItemSpawnPositionTypes;
    }

    private static void FixLungPropBlankReferences(On.LungProp.orig_Start orig, LungProp self)
    {
        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (!enemyInfo.Key.IsVanilla())
                continue;

            if (self.radMechEnemyType == null)
                continue;

            if ((enemyInfo.EnemyType.name == self.radMechEnemyType.name || enemyInfo.EnemyType.enemyName == self.radMechEnemyType.enemyName) && self.radMechEnemyType != enemyInfo.EnemyType)
            {
                self.radMechEnemyType = enemyInfo.EnemyType;
                break;
            }
        }
        orig(self);
    }

    private static void FixKiwiBlankReferences(On.GiantKiwiAI.orig_Start orig, GiantKiwiAI self)
    {
        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (!enemyInfo.Key.IsVanilla())
                continue;

            if (self.baboonHawkType == null)
                continue;

            if ((enemyInfo.EnemyType.name == self.baboonHawkType.name || enemyInfo.EnemyType.enemyName == self.baboonHawkType.enemyName) && self.baboonHawkType != enemyInfo.EnemyType)
            {
                self.baboonHawkType = enemyInfo.EnemyType;
                break;
            }
        }
        orig(self);
    }

    private static void FixButlerBlankReferences(On.ButlerEnemyAI.orig_Start orig, ButlerEnemyAI self)
    {
        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (!enemyInfo.Key.IsVanilla())
                continue;

            if (self.butlerBeesEnemyType == null)
                continue;

            if ((enemyInfo.EnemyType.name == self.butlerBeesEnemyType.name || enemyInfo.EnemyType.enemyName == self.butlerBeesEnemyType.enemyName) && self.butlerBeesEnemyType != enemyInfo.EnemyType)
            {
                self.butlerBeesEnemyType = enemyInfo.EnemyType;
                break;
            }
        }
        orig(self);
    }

    private static void FixNestBlankReferences(RuntimeILReferenceBag.FastDelegateInvokers.Action<EnemyAINestSpawnObject> orig, EnemyAINestSpawnObject self)
    {
        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (!enemyInfo.Key.IsVanilla())
                continue;

            if (self.enemyType == null)
                continue;

            if ((enemyInfo.EnemyType.name == self.enemyType.name || enemyInfo.EnemyType.enemyName == self.enemyType.enemyName) && self.enemyType != enemyInfo.EnemyType)
            {
                self.enemyType = enemyInfo.EnemyType;
                break;
            }
        }
        orig(self);
    }

    private static void FixHauntedMaskBlankReferences(RuntimeILReferenceBag.FastDelegateInvokers.Action<HauntedMaskItem> orig, HauntedMaskItem self)
    {
        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (!enemyInfo.Key.IsVanilla())
                continue;

            if (self.mimicEnemy == null)
                continue;

            if ((enemyInfo.EnemyType.name == self.mimicEnemy.name || enemyInfo.EnemyType.enemyName == self.mimicEnemy.enemyName) && self.mimicEnemy != enemyInfo.EnemyType)
            {
                self.mimicEnemy = enemyInfo.EnemyType;
                break;
            }
        }
        orig(self);
    }

    private static void FixItemSpawnPositionTypes()
    {
        List<ItemGroup> vanillaItemGroups = new();
        foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
        {
            if (!itemInfo.Key.IsVanilla())
                continue;

            foreach (ItemGroup itemGroup in itemInfo.Item.spawnPositionTypes)
            {
                if (itemGroup == null || vanillaItemGroups.Contains(itemGroup))
                    continue;

                vanillaItemGroups.Add(itemGroup);
            }
        }

        foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
        {
            foreach (ItemGroup itemGroup in itemInfo.Item.spawnPositionTypes.ToArray())
            {
                if (itemGroup == null)
                    continue;

                foreach (ItemGroup vanillaItemGroup in vanillaItemGroups)
                {
                    if (vanillaItemGroup == null)
                        continue;

                    if (itemGroup.name == vanillaItemGroup.name)
                    {
                        Debuggers.Items?.Log($"Replacing non-vanilla spawn type {itemGroup.name} with original.");
                        itemInfo.Item.spawnPositionTypes.Remove(itemGroup);
                        itemInfo.Item.spawnPositionTypes.Add(vanillaItemGroup);
                    }
                }
            }
        }
    }

    private static void DoSoundFixes(On.MenuManager.orig_Start orig, MenuManager self)
    {
        orig(self);

        AudioSource? menuManagerAudioSource = self.gameObject.GetComponent<AudioSource>();
        if (menuManagerAudioSource == null)
        {
            return;
        }
        AudioMixer audioMixer = menuManagerAudioSource.outputAudioMixerGroup.audioMixer;
        foreach (GameObject prefabToFix in soundPrefabsToFix)
        {
            AudioSource[] audioSourcesToFix = prefabToFix.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource audioSource in audioSourcesToFix)
            {
                if (audioSource.outputAudioMixerGroup == null || audioSource.outputAudioMixerGroup.audioMixer.name != "NonDiagetic") // huh why does LL ignore it if it's null or not NonDiagetic?
                    continue;

                AudioMixerGroup? audioMixerGroup = audioMixer.FindMatchingGroups(audioSource.outputAudioMixerGroup.name)[0];
                if (audioMixerGroup == null)
                    continue;

                audioSource.outputAudioMixerGroup = audioMixerGroup;
                Debuggers.Sounds?.Log("Set mixer group for " + audioSource.name + " in " + prefabToFix.name + " to NonDiagetic:" + audioMixerGroup.name);
            }
        }

        soundPrefabsToFix.Clear();
    }

    private static void FixTileSetSockets()
    {
        Dictionary<string, DoorwaySocket> mapped = new(); // improve performance
        foreach (DawnDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
        {
            foreach (DoorwaySocket socket in dungeonInfo.Sockets)
            {
                mapped[socket.name] = socket;
            }
        }

        foreach (GameObject tile in tilesToFixSockets)
        {
            Doorway[] doorways = tile.GetComponentsInChildren<Doorway>();

            foreach (Doorway doorway in doorways)
            {
                doorway.socket = mapped[doorway.socket.name];
            }
        }

        tilesToFixSockets.Clear();
    }

    private static void AddNetworkPrefabToNetworkConfig(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
    {
        orig(self);
        foreach (GameObject networkPrefab in networkPrefabsToAdd)
        {
            if (NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(networkPrefab))
                continue;

            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        }
        networkPrefabsToAdd.Clear();
    }
}