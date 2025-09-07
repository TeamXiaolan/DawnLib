namespace Dawn.Internal;

static class SaveDataPatch
{
    internal static void Init()
    {
        On.DeleteFileButton.DeleteFile += ResetSaveFile;
        On.GameNetworkManager.SaveItemsInShip += SaveData;
        On.GameNetworkManager.ResetSavedGameValues += ResetSaveFile;
        On.StartOfRound.AutoSaveShipData += SaveData;
    }

    private static void SaveData(On.StartOfRound.orig_AutoSaveShipData orig, StartOfRound self)
    {
        orig(self);
        DawnNetworker.Instance?.SaveData();
    }

    private static void ResetSaveFile(On.GameNetworkManager.orig_ResetSavedGameValues orig, GameNetworkManager self)
    {
        orig(self);
        ES3Settings settings;
        if (DawnNetworker.Instance != null)
        {
            settings = DawnNetworker.Instance.SaveSettings;
        }
        else
        {
            settings = new ES3Settings($"DawnLib{GameNetworkManager.Instance.currentSaveFileName}", ES3.EncryptionType.None);
        }
        DawnNetworker.ResetData(settings);

        PersistentDataContainer container = DawnNetworker.Instance?.ContractContainer ?? DawnNetworker.CreateContractContainer(GameNetworkManager.Instance.currentSaveFileName);
        container.Clear();
    }

    private static void SaveData(On.GameNetworkManager.orig_SaveItemsInShip orig, GameNetworkManager self)
    {
        orig(self);
        DawnNetworker.Instance?.SaveData();
    }

    private static void ResetSaveFile(On.DeleteFileButton.orig_DeleteFile orig, DeleteFileButton self)
    {
        orig(self);
        ES3Settings settings;
        if (DawnNetworker.Instance != null)
        {
            settings = DawnNetworker.Instance.SaveSettings;
        }
        else
        {
            settings = new ES3Settings($"DawnLibLCSaveFile{self.fileToDelete + 1}", ES3.EncryptionType.None);
        }
        DawnNetworker.ResetData(settings);
        
        PersistentDataContainer container = DawnNetworker.Instance?.ContractContainer ?? DawnNetworker.CreateContractContainer(GameNetworkManager.Instance.currentSaveFileName);
        container.Clear();
        
        container = DawnNetworker.Instance?.SaveContainer ?? DawnNetworker.CreateSaveContainer(GameNetworkManager.Instance.currentSaveFileName);
        container.Clear();
    }
}