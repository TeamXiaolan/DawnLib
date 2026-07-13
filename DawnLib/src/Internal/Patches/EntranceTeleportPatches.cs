using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Dawn.Internal;

public static class EntranceTeleportPatches
{
    public static void Init()
    {
        On.EntranceTeleport.Awake += EntranceTeleport_Awake;
        new Hook(AccessTools.DeclaredMethod(typeof(EntranceTeleport), "OnDestroy"), EntranceTeleport_OnDestroy);
    }

    private static void EntranceTeleport_Awake(On.EntranceTeleport.orig_Awake orig, EntranceTeleport self)
    {
        DawnNetworker.EntranceTeleports.Add(self);
        orig(self);
    }

    private static void EntranceTeleport_OnDestroy(RuntimeILReferenceBag.FastDelegateInvokers.Action<EntranceTeleport> orig, EntranceTeleport self)
    {
        DawnNetworker.EntranceTeleports.Remove(self);
        self.OnDestroy();
        orig(self);
    }
}