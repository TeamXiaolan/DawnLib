using System;
using System.IO;
using Dawn.Utils;
using Unity.Netcode;

namespace Dawn.Internal;

public class DawnNetworker : NetworkSingleton<DawnNetworker>
{
    internal event Action OnSave = delegate { };
    internal PersistentDataContainer SaveContainer { get; private set; }
    internal PersistentDataContainer ContractContainer { get; private set; }

    private void Awake()
    {
        string saveId = GameNetworkManager.Instance.currentSaveFileName;
        SaveContainer = CreateSaveContainer(saveId);
        ContractContainer = CreateContractContainer(saveId);
    }

    internal static PersistentDataContainer CreateSaveContainer(string id)
    {
        PersistentDataContainer dataContainer = new PersistentDataContainer(Path.Combine(PersistentDataHandler.RootPath, $"Save{id}"));
        return dataContainer;
    }

    internal static PersistentDataContainer CreateContractContainer(string id)
    {
        return new PersistentDataContainer(Path.Combine(PersistentDataHandler.RootPath, $"Contract{id}"));
    }

    internal void SaveData()
    {
        OnSave();
        if (!NetworkManager.Singleton.IsHost)
            return;

        if (!DawnConfig.DisableDawnItemSaving.Value)
        {
            ItemSaveDataHandler.SaveAllItems(DawnLib.GetCurrentContract()!);
        }

        if (!DawnConfig.DisableDawnUnlockableSaving.Value)
        {
            UnlockableSaveDataHandler.SaveAllUnlockables(DawnLib.GetCurrentContract()!);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BroadcastDisplayTipServerRpc(HUDDisplayTip displayTip)
    {
        BroadcastDisplayTipClientRpc(displayTip);
    }

    [ClientRpc]
    public void BroadcastDisplayTipClientRpc(HUDDisplayTip displayTip)
    {
        HUDManager.Instance.DisplayTip(displayTip);
    }
}