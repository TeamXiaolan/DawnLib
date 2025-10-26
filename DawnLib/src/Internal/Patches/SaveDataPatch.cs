
using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Dawn.Internal;

[HarmonyPatch]
static class SaveDataPatch
{
    internal static PersistentDataContainer? contractContainer;

    internal static void Init()
    {
        On.DeleteFileButton.DeleteFile += ResetSaveFile;
        On.GameNetworkManager.SaveItemsInShip += SaveData;
        On.GameNetworkManager.ResetSavedGameValues += ResetSaveFile;
        On.StartOfRound.AutoSaveShipData += SaveData;

        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(PlaceableShipObject), "Awake"), OnPlaceableShipObjectAwake));
        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(PlaceableShipObject), "OnDestroy"), OnPlaceableShipObjectOnDestroy));
    }

    private static void OnPlaceableShipObjectAwake(RuntimeILReferenceBag.FastDelegateInvokers.Action<PlaceableShipObject> orig, PlaceableShipObject self)
    {
        UnlockableSaveDataHandler.placeableShipObjects.Add(self);
        orig(self);
    }

    private static void OnPlaceableShipObjectOnDestroy(RuntimeILReferenceBag.FastDelegateInvokers.Action<PlaceableShipObject> orig, PlaceableShipObject self)
    {
        UnlockableSaveDataHandler.placeableShipObjects.Remove(self);
        orig(self);
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.LoadUnlockables)), HarmonyPrefix]
    static bool LoadUnlockables()
    {
        if (DawnConfig.DisableDawnUnlockableSaving)
        {
            return true;
        }

        contractContainer = DawnLib.GetCurrentContract() ?? DawnNetworker.CreateContractContainer(GameNetworkManager.Instance.currentSaveFileName);
        UnlockableSaveDataHandler.LoadSavedUnlockables(contractContainer);
        return false;
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.LoadShipGrabbableItems)), HarmonyPrefix]
    static bool LoadShipGrabbableItems()
    {
        if (DawnConfig.DisableDawnItemSaving)
        {
            return true;
        }
        
        contractContainer = DawnLib.GetCurrentContract() ?? DawnNetworker.CreateContractContainer(GameNetworkManager.Instance.currentSaveFileName);
        ItemSaveDataHandler.LoadSavedItems(contractContainer);
        return false;
    }
    
    private static void SaveData(On.StartOfRound.orig_AutoSaveShipData orig, StartOfRound self)
    {
        orig(self);
        DawnNetworker.Instance?.SaveData();
    }

    private static void SaveData(On.GameNetworkManager.orig_SaveItemsInShip orig, GameNetworkManager self)
    {
        if (DawnConfig.DisableDawnItemSaving)
        {
            orig(self);
        }
        DawnNetworker.Instance?.SaveData();
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
        PersistentDataContainer contractContainer = DawnNetworker.CreateContractContainer($"LCSaveFile{self.fileToDelete+1}");
        contractContainer.Clear();

        PersistentDataContainer saveContainer = DawnNetworker.CreateSaveContainer($"LCSaveFile{self.fileToDelete+1}");
        saveContainer.Clear();
    }
}