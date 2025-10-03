using System.Collections.Generic;
using DunGen;
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
        LethalContent.Dungeons.OnFreeze += FixTileSetSockets;
        LethalContent.Items.OnFreeze += FixItemSpawnPositionTypes;
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