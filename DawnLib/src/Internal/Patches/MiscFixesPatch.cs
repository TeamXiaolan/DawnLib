using System.Collections.Generic;
using DunGen;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

namespace CodeRebirthLib.Internal;

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
        foreach (CRDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
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