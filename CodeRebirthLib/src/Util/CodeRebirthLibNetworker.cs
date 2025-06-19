using System.Collections.Generic;
using CodeRebirthLib.ContentManagement.Unlockables.Progressive;
using CodeRebirthLib.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib.Util;
public class CodeRebirthLibNetworker : NetworkSingleton<CodeRebirthLibNetworker>
{
    internal static EntranceTeleport[] _entrancePoints = [];
    public static IReadOnlyList<EntranceTeleport> EntrancePoints => _entrancePoints;
    internal ES3Settings SaveSettings;
    internal System.Random CRLibRandom = new();

    private void Awake()
    {
        if (StartOfRound.Instance == null)
            return;

        CRLibRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 6969);
        StartOfRound.Instance.StartNewRoundEvent.AddListener(OnNewRoundStart);
        SaveSettings = new($"CRLib{GameNetworkManager.Instance.currentSaveFileName}", ES3.EncryptionType.None);
        ProgressiveUnlockableHandler.LoadAll(SaveSettings);
    }

    internal void SaveCodeRebirthLibData()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        ProgressiveUnlockableHandler.SaveAll(SaveSettings);
    }

    internal static void ResetCodeRebirthLibData(ES3Settings saveSettings)
    {
        ES3.DeleteFile(saveSettings);
    }

    private void OnNewRoundStart()
    {
        _entrancePoints = FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.InstanceID);
        foreach (var entrance in _entrancePoints)
        {
            if (!entrance.FindExitPoint())
            {
                CodeRebirthLibPlugin.Logger.LogError("Something went wrong in the generation of the fire exits! (ignorable if EntranceTeleportOptimisation is installed)");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    internal void BroadcastDisplayTipServerRPC(HUDDisplayTip displayTip)
    {
        BroadcastDisplayTipClientRPC(displayTip);
    }

    [ClientRpc]
    void BroadcastDisplayTipClientRPC(HUDDisplayTip displayTip)
    {
        HUDManager.Instance.DisplayTip(displayTip);
    }
}