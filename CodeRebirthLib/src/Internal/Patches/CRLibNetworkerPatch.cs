using CodeRebirthLib.CRMod;
using CodeRebirthLib.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeRebirthLib;

static class CRLibNetworkerPatch
{
    internal static void Init()
    {
        On.StartOfRound.Start += CreateCRLibNetworker;
    }

    private static void CreateCRLibNetworker(On.StartOfRound.orig_Start orig, StartOfRound self)
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