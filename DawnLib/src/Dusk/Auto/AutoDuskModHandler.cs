using System.IO;
using BepInEx;
using Dawn.Internal;
using UnityEngine;

namespace Dawn.Dusk;

public class AutoDuskModHandler
{
    public static void AutoRegisterMods()
    {
        foreach (string path in Directory.GetFiles(Paths.PluginPath, "*.duskmod", SearchOption.AllDirectories))
        {
            AssetBundle mainBundle = AssetBundle.LoadFromFile(path);
            Debuggers.AssetLoading?.Log($"{mainBundle.name} contains these objects: {string.Join(",", mainBundle.GetAllAssetNames())}");

            DuskModInformation[] modInformation = mainBundle.LoadAllAssets<DuskModInformation>();
            if (modInformation.Length == 0)
            {
                DawnPlugin.Logger.LogError($".duskmod bundle: '{Path.GetFileName(path)}' does not have a 'Mod Information' file!");
                continue;
            }

            if (modInformation.Length > 1)
            {
                DawnPlugin.Logger.LogError($".duskmod bundle: '{Path.GetFileName(path)}' has multiple 'Mod Information' files! Only the first one will be used.");
            }

            DawnPlugin.Logger.LogInfo($"AuthorName: {modInformation[0].AuthorName}, ModName: {modInformation[0].ModName}, Version: {modInformation[0].Version}");
            DuskMod.RegisterNoCodeMod(modInformation[0], mainBundle, Path.GetDirectoryName(path)!);
        }
    }
}