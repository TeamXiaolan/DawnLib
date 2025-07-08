using System.IO;
using BepInEx;
using CodeRebirthLib.AssetManagement;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement;
public class DefaultContentHandler : ContentHandler
{
    public DefaultContentHandler(CRMod mod) : base(mod)
    {
        foreach (AssetBundleData bundleData in mod.Content.assetBundles)
        {
            mod.Logger?.LogDebug($"Trying to register bundle: {bundleData}");
            
            if (!IsContentEnabled(bundleData))
                continue;

            if (!mod.TryGetRelativeFile(out string path, "Assets", bundleData.assetBundleName))
            {
                mod.Logger?.LogError($"The bundle: {bundleData.configName} is not defined at plugins/{Path.GetRelativePath(Paths.PluginPath, path)}.");

                if (mod.TryGetRelativeFile(out string incorrectPath, bundleData.assetBundleName)) // check if it is instead next to the .crmod file
                {
                    mod.Logger?.LogError($"The bundle is instead defined at plugins/{Path.GetRelativePath(Paths.PluginPath, incorrectPath)}. It should be in an Assets/ subfolder.");
                }
                
                continue;
            }
            
            DefaultBundle bundle = new(AssetBundle.LoadFromFile(path));
            bundle.AssetBundleData = bundleData;
            LoadAllContent(bundle);
        }
    }
}