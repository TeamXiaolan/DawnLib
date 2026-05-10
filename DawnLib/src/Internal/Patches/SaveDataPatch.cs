using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dawn.Utils;
using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Unity.Netcode;
using UnityEngine;

namespace Dawn.Internal;

[HarmonyPatch]
static class SaveDataPatch
{
    internal static void Init()
    {
        On.MenuManager.Start += LCBetterSaveInit;
        On.MenuManager.Start += SetLastDawnVersion;
        On.MenuManager.Start += ResetInvalidSaveFiles;
        On.DeleteFileButton.DeleteFile += ResetSaveFile;
        On.GameNetworkManager.ResetSavedGameValues += ResetSaveFile;
        On.StartOfRound.AutoSaveShipData += SaveData;

        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(PlaceableShipObject), "Awake"), OnPlaceableShipObjectAwake));
        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(PlaceableShipObject), "OnDestroy"), OnPlaceableShipObjectOnDestroy));
    }

    private static readonly Regex SaveFileRegex = new Regex(@"^ContractLCSaveFile\d+$");
    private static List<string> GetDawnSaveFiles()
    {
        return Directory.EnumerateFiles(PersistentDataHandler.RootPath, "ContractLCSaveFile*", SearchOption.TopDirectoryOnly)
            .Where(file =>
            {
                string fileName = Path.GetFileName(file);
                return SaveFileRegex.IsMatch(fileName);
            })
            .ToList();
    }

    private static void ResetInvalidSaveFiles(On.MenuManager.orig_Start orig, MenuManager self)
    {
        if (!DawnPlugin.PersistentData.TryGet(DawnKeys.LastVersion, out string? lastLaunchVersion) || Version.Parse(lastLaunchVersion) <= Version.Parse("0.9.18")) // 0.9.18 is the version before the saves thing was created
        {
            orig(self);
            return;
        }

        string DawnSaveKey = DawnKeys.DawnSave.ToString();
        foreach (string dawnSaveFile in GetDawnSaveFiles())
        {
            int saveNumber = int.Parse(dawnSaveFile[^1..]);
            string fileName = $"LCSaveFile{saveNumber}";
            string vanillaFilePath = Path.Combine(Application.persistentDataPath, fileName);

            bool fileExists = File.Exists(vanillaFilePath);
            if (fileExists)
            {
                continue;
            }

            ES3Settings settings = new ES3Settings(vanillaFilePath);
            if (ES3.KeyExists(DawnSaveKey, settings))
            {
                continue;
            }

            PersistentDataContainer contractContainer = DawnNetworker.CreateContractContainer(fileName);
            contractContainer.Clear();

            PersistentDataContainer saveContainer = DawnNetworker.CreateSaveContainer(fileName);
            saveContainer.Clear();
            Debuggers.SaveManager?.Log($"Clearing potential invalid save: {dawnSaveFile}.");

            if (fileExists)
            {
                ES3.Save(DawnSaveKey, true, settings);
            }
        }
        orig(self);
    }

    private static void SetLastDawnVersion(On.MenuManager.orig_Start orig, MenuManager self)
    {
        orig(self);
        DawnPlugin.PersistentData.Set(DawnKeys.LastVersion, MyPluginInfo.PLUGIN_VERSION);
    }

    private static void LCBetterSaveInit(On.MenuManager.orig_Start orig, MenuManager self)
    {
        orig(self);
        if (LCBetterSaveCompat.Enabled)
        {
            LCBetterSaveCompat.Init();
        }
    }

    private static void OnPlaceableShipObjectAwake(RuntimeILReferenceBag.FastDelegateInvokers.Action<PlaceableShipObject> orig, PlaceableShipObject self)
    {
        UnlockableSaveDataHandler.placeableShipObjects.Add(self);
        orig(self);
        if (self.parentObject == null)
        {
            DawnPlugin.Logger.LogError($"Parent object is null for object: {self.gameObject.name}");
            return;
        }

        if (self.parentObject.NetworkObject == null)
        {
            DawnPlugin.Logger.LogError($"Network object is null for object: {self.parentObject.gameObject.name}");
            return;
        }

        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        self.parentObject.NetworkObject.OnSpawn(() =>
        {
            if (!self.parentObject.NetworkObject.TrySetParent(StartOfRoundRefs.Instance.shipAnimatorObject, true))
            {
                DawnPlugin.Logger.LogError($"Parenting of object: {self.parentObject.gameObject.name} failed.");
            }
        });
    }

    private static void OnPlaceableShipObjectOnDestroy(RuntimeILReferenceBag.FastDelegateInvokers.Action<PlaceableShipObject> orig, PlaceableShipObject self)
    {
        UnlockableSaveDataHandler.placeableShipObjects.Remove(self);
        orig(self);
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.LoadUnlockables)), HarmonyPrefix]
    static bool LoadUnlockables()
    {
        if (DawnConfig.DisableDawnUnlockableSaving.Value)
        {
            return true;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            PersistentDataContainer contractContainer = DawnLib.GetCurrentContract() ?? DawnNetworker.CreateContractContainer(GameNetworkManager.Instance.currentSaveFileName);
            UnlockableSaveDataHandler.LoadSavedUnlockables(contractContainer);
        }

        return false;
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.LoadShipGrabbableItems)), HarmonyPrefix]
    static bool LoadShipGrabbableItems()
    {
        if (DawnConfig.DisableDawnItemSaving.Value)
        {
            return true;
        }

        PersistentDataContainer contractContainer = DawnLib.GetCurrentContract() ?? DawnNetworker.CreateContractContainer(GameNetworkManager.Instance.currentSaveFileName);
        ItemSaveDataHandler.LoadSavedItems(contractContainer);
        return false;
    }

    private static void SaveData(On.StartOfRound.orig_AutoSaveShipData orig, StartOfRound self)
    {
        orig(self);
        DawnNetworker.Instance?.SaveData();
    }

    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SaveItemsInShip)), HarmonyPrefix]
    static bool SaveData()
    {
        DawnNetworker.Instance?.SaveData();
        if (DawnConfig.DisableDawnItemSaving.Value)
        {
            return true;
        }
        return false;
    }

    private static void ResetSaveFile(On.GameNetworkManager.orig_ResetSavedGameValues orig, GameNetworkManager self)
    {
        orig(self);
        PersistentDataContainer container = DawnNetworker.Instance?.ContractContainer ?? DawnNetworker.CreateContractContainer(self.currentSaveFileName);
        container.Clear();
    }

    private static void ResetSaveFile(On.DeleteFileButton.orig_DeleteFile orig, DeleteFileButton self)
    {
        orig(self);
        PersistentDataContainer contractContainer = DawnNetworker.CreateContractContainer($"LCSaveFile{self.fileToDelete + 1}");
        contractContainer.Clear();

        PersistentDataContainer saveContainer = DawnNetworker.CreateSaveContainer($"LCSaveFile{self.fileToDelete + 1}");
        saveContainer.Clear();
    }
}