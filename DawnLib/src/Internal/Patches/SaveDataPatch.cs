
using System;
using Dawn.Utils;
using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Unity.Netcode;

namespace Dawn.Internal;

[HarmonyPatch]
static class SaveDataPatch
{
    internal static void Init()
    {
        On.MenuManager.Start += LCBetterSaveInit;
        On.MenuManager.Start += SetLastDawnVersion;
        On.DeleteFileButton.DeleteFile += ResetSaveFile;
        On.GameNetworkManager.ResetSavedGameValues += ResetSaveFile;
        On.StartOfRound.AutoSaveShipData += SaveData;

        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(PlaceableShipObject), "Awake"), OnPlaceableShipObjectAwake));
        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(PlaceableShipObject), "OnDestroy"), OnPlaceableShipObjectOnDestroy));
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