using System.IO;
using BepInEx;
using CodeRebirthLib.Internal;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

public class AutoCRModHandler
{
    public static void AutoRegisterMods()
    {
        foreach (string path in Directory.GetFiles(Paths.PluginPath, "*.crmod", SearchOption.AllDirectories))
        {
            AssetBundle mainBundle = AssetBundle.LoadFromFile(path);
            Debuggers.AssetLoading?.Log($"{mainBundle.name} contains these objects: {string.Join(",", mainBundle.GetAllAssetNames())}");

            CRModInformation[] modInformation = mainBundle.LoadAllAssets<CRModInformation>();
            if (modInformation.Length == 0)
            {
                CodeRebirthLibPlugin.Logger.LogError($".crmod bundle: '{Path.GetFileName(path)}' does not have a 'Mod Information' file!");
                continue;
            }

            if (modInformation.Length > 1)
            {
                CodeRebirthLibPlugin.Logger.LogError($".crmod bundle: '{Path.GetFileName(path)}' has multiple 'Mod Information' files! Only the first one will be used.");
            }

            CodeRebirthLibPlugin.Logger.LogInfo($"AuthorName: {modInformation[0].AuthorName}, ModName: {modInformation[0].ModName}, Version: {modInformation[0].Version}");
            CRMod.RegisterNoCodeMod(modInformation[0], mainBundle, Path.GetDirectoryName(path)!);
        }
    }
}