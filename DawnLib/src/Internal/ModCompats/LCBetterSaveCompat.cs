using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Dawn.Internal;

static class LCBetterSaveCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(LCBetterSaves.PluginInfo.PLUGIN_GUID);

    private static bool alreadyHooked = false;
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Init()
    {
        if (alreadyHooked)
        {
            return;
        }

        alreadyHooked = true;
        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(DeleteFileButton_BetterSaves), "DeleteFile"), OnFileDeleteSaveDataPatch));
    }

    private static void OnFileDeleteSaveDataPatch(RuntimeILReferenceBag.FastDelegateInvokers.Action<DeleteFileButton_BetterSaves> orig, DeleteFileButton_BetterSaves self)
    {
        PersistentDataContainer contractContainer = DawnNetworker.CreateContractContainer($"LCSaveFile{self.fileToDelete}");
        contractContainer.Clear();

        PersistentDataContainer saveContainer = DawnNetworker.CreateSaveContainer($"LCSaveFile{self.fileToDelete}");
        saveContainer.Clear();
        orig(self);
    }
}