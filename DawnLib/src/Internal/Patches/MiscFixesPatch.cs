using System.Collections.Generic;
using Dawn.Utils;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

namespace Dawn.Internal;

static class MiscFixesPatch
{
    internal static HashSet<GameObject> networkPrefabsToAdd = new();
    internal static HashSet<GameObject> soundPrefabsToFix = new();
    internal static HashSet<GameObject> tilesToFixSockets = new();

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
        IL.GameNetcodeStuff.PlayerControllerB.DestroyItemInSlot += FixOutOfBoundsError;
    }

    private static void FixOutOfBoundsError(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.After,
            il => il.MatchLdarg(0),
            il => il.MatchLdfld<PlayerControllerB>(nameof(PlayerControllerB.ItemSlots)),
            il => il.MatchLdarg(1),
            il => il.MatchLdelemRef(),
            il => il.MatchStloc(0),
            il => il.MatchLdarg(0)
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match GameNetcodeStuff.PlayerControllerB.DestroyItemInSlot (1) IL.");
            return;
        }

        cursor.Emit(OpCodes.Ldloc, 0);
        cursor.EmitDelegate((PlayerControllerB playerControllerB, GrabbableObject grabbableObject) =>
        {
            playerControllerB.carryWeight = Mathf.Clamp(playerControllerB.carryWeight - (grabbableObject.itemProperties.weight - 1f), 1f, 10f);
            if (playerControllerB.IsLocalPlayer())
            {
                StartOfRoundRefs.Instance.SendChangedWeightEvent();
            }
        });
        cursor.Emit(OpCodes.Ldarg, 0);

        if (!cursor.TryGotoNext(MoveType.Before,
            il => il.MatchLdarg(0),
            il => il.MatchLdarg(0),
            il => il.MatchLdfld<PlayerControllerB>(nameof(PlayerControllerB.carryWeight)),
            il => il.MatchLdarg(0),
            il => il.MatchLdfld<PlayerControllerB>(nameof(PlayerControllerB.currentlyHeldObjectServer)),
            il => il.MatchLdfld<GrabbableObject>(nameof(GrabbableObject.itemProperties)),
            il => il.MatchLdfld<Item>(nameof(Item.weight)),
            il => il.MatchLdcR4(1),
            il => il.MatchSub(),
            il => il.MatchSub(),
            il => il.MatchLdcR4(1),
            il => il.MatchLdcR4(10),
            il => il.MatchCall<Mathf>(nameof(Mathf.Clamp)),
            il => il.MatchStfld<PlayerControllerB>(nameof(PlayerControllerB.carryWeight))
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match GameNetcodeStuff.PlayerControllerB.DestroyItemInSlot (2) IL.");
            return;
        }

        cursor.RemoveRange(14);

        if (!cursor.TryGotoNext(MoveType.Before,
            il => il.MatchLdfld<HUDManager>(nameof(HUDManager.itemSlotIcons)),
            il => il.MatchLdarg(1),
            il => il.MatchLdelemRef(),
            il => il.MatchLdcI4(0),
            il => il.MatchCallvirt<Behaviour>($"set_{nameof(Behaviour.enabled)}")
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match GameNetcodeStuff.PlayerControllerB.DestroyItemInSlot (3) IL.");
            return;
        }

        cursor.RemoveRange(5);
        cursor.Emit(OpCodes.Ldarg, 1);
        cursor.EmitDelegate((HUDManager hudManager, int itemSlot) =>
        {
            if (itemSlot == 50)
            {
                hudManager.itemOnlySlotIcon.enabled = false;
            }
            else
            {
                hudManager.itemSlotIcons[itemSlot].enabled = false;
            }
        });
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
            if (itemInfo.Key.IsVanilla())
                continue;

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
            Debuggers.Dungeons?.Log($"Mapping sockets for {dungeonInfo.Key}");
            foreach (DoorwaySocket socket in dungeonInfo.Sockets)
            {
                Debuggers.Dungeons?.Log($" - {socket.name}");
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