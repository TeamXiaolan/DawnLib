
namespace Dawn.Internal;

static class SaveDataPatch
{
    internal static PersistentDataContainer? contractContainer;

    internal static void Init()
    {
        On.DeleteFileButton.DeleteFile += ResetSaveFile;
        On.GameNetworkManager.SaveItemsInShip += SaveData;
        On.GameNetworkManager.ResetSavedGameValues += ResetSaveFile;
        On.StartOfRound.AutoSaveShipData += SaveData;
        On.StartOfRound.LoadShipGrabbableItems += LoadShipGrabbableItems;
    }

    private static void LoadShipGrabbableItems(On.StartOfRound.orig_LoadShipGrabbableItems orig, StartOfRound self)
    {
        // TODO this errors??
        contractContainer = DawnLib.GetCurrentContract() ?? DawnNetworker.CreateContractContainer(GameNetworkManager.Instance.currentSaveFileName);
        ItemSaveDataHandler.LoadSavedItems(contractContainer);
        // orig(self);
    }

    private static void SaveData(On.StartOfRound.orig_AutoSaveShipData orig, StartOfRound self)
    {
        orig(self);
        DawnNetworker.Instance?.SaveData();
    }

    private static void SaveData(On.GameNetworkManager.orig_SaveItemsInShip orig, GameNetworkManager self)
    {
        // orig(self);
        DawnNetworker.Instance?.SaveData();
    }

    private static void ResetSaveFile(On.GameNetworkManager.orig_ResetSavedGameValues orig, GameNetworkManager self)
    {
        orig(self);
        PersistentDataContainer container = DawnNetworker.Instance?.ContractContainer ?? DawnNetworker.CreateContractContainer(GameNetworkManager.Instance.currentSaveFileName);
        container.Clear();
    }

    private static void ResetSaveFile(On.DeleteFileButton.orig_DeleteFile orig, DeleteFileButton self)
    {
        orig(self);
        PersistentDataContainer contractContainer = DawnNetworker.Instance?.ContractContainer ?? DawnNetworker.CreateContractContainer(GameNetworkManager.Instance.currentSaveFileName);
        contractContainer.Clear();

        PersistentDataContainer saveContainer = DawnNetworker.Instance?.SaveContainer ?? DawnNetworker.CreateSaveContainer(GameNetworkManager.Instance.currentSaveFileName);
        saveContainer.Clear();
    }
}