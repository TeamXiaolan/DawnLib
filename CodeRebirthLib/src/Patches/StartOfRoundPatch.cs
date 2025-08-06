using CodeRebirthLib.ContentManagement.Achievements;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;
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
        On.StartOfRound.Start += StartOfRound_Start;
        On.StartOfRound.SetPlanetsWeather += StartOfRound_SetPlanetsWeather;
        On.StartOfRound.AutoSaveShipData += StartOfRound_AutoSaveShipData;
    }

    private static void StartOfRound_AutoSaveShipData(On.StartOfRound.orig_AutoSaveShipData orig, StartOfRound self)
    {
        orig(self);
        CRAchievementHandler.SaveAll();
        CodeRebirthLibNetworker.Instance?.SaveCodeRebirthLibData();
    }

    private static void StartOfRound_SetPlanetsWeather(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedplayersonserver)
    {
        orig(self, connectedplayersonserver);
        CREnemyDefinition.UpdateAllWeights();
        CRItemDefinition.UpdateAllWeights();
    }

    private static void StartOfRound_Start(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        orig(self);
        MoreLayerMasks.Init();

        self.NetworkObject.OnSpawn(() =>
        {
            if (self.IsServer || self.IsHost)
            {
                if (!CodeRebirthLibNetworker.Instance)
                {
                    GameObject networkerInstance = Object.Instantiate(CodeRebirthLibPlugin.Main.NetworkerPrefab);
                    SceneManager.MoveGameObjectToScene(networkerInstance, self.gameObject.scene);
                    networkerInstance.GetComponent<NetworkObject>().Spawn();
                }
            }

            if (AchievementUIGetCanvas.Instance == null) Object.Instantiate(CodeRebirthLibPlugin.Main.AchievementGetUICanvasPrefab);
        });
    }
}