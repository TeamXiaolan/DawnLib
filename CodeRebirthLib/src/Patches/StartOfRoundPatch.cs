using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.Extensions;
using CodeRebirthLib.Util;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeRebirthLib.Patches;
static class StartOfRoundPatch
{
    internal static void Init()
    {
        On.StartOfRound.Start += StartOfRoundOnStart;
        On.StartOfRound.SetPlanetsWeather += RefreshEnemyWeights;
        On.StartOfRound.AutoSaveShipData += StartOfRound_AutoSaveShipData;
    }

    private static void StartOfRound_AutoSaveShipData(On.StartOfRound.orig_AutoSaveShipData orig, StartOfRound self)
    {
        orig(self);
        CodeRebirthLibNetworker.Instance?.SaveCodeRebirthLibData();
    }

    private static void RefreshEnemyWeights(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedplayersonserver)
    {
        orig(self, connectedplayersonserver);
        CREnemyDefinition.UpdateAllWeights();
    }

    private static void StartOfRoundOnStart(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        CREnemyDefinition.CreateMoonAttributeStacks();
        orig(self);
        MoreLayerMasks.Init();
        
        self.NetworkObject.OnSpawn(() =>
        {
            if (self.IsServer || self.IsHost)
            {
                if (!CodeRebirthLibNetworker.Instance)
                {
                    GameObject networkerInstance = Object.Instantiate(CodeRebirthLibNetworker.prefab);
                    SceneManager.MoveGameObjectToScene(networkerInstance, self.gameObject.scene);
                    networkerInstance.GetComponent<NetworkObject>().Spawn();
                }
            }
        });
    }
}