using System.IO;
using BepInEx;
using UnityEngine;

namespace Dusk;

public class DefaultContentHandler : ContentHandler
{
    public DefaultContentHandler(DuskMod mod) : base(mod)
    {
        mod.Logger?.LogDebug($"Trying to register bundle: {mod.Content.name} with {mod.Content.assetBundles.Count} assets.");
        foreach (AssetBundleData bundleData in mod.Content.assetBundles)
        {
            if (!IsContentEnabled(bundleData))
                continue;

            if (!mod.TryGetRelativeFile(out string path, "Assets", bundleData.assetBundleName))
            {
                mod.Logger?.LogError($"The bundle: {bundleData.configName} is not defined at plugins/{Path.GetRelativePath(Paths.PluginPath, path)}.");

                if (mod.TryGetRelativeFile(out string incorrectPath, bundleData.assetBundleName)) // check if it is instead next to the .duskmod file
                {
                    mod.Logger?.LogError($"The bundle is instead defined at plugins/{Path.GetRelativePath(Paths.PluginPath, incorrectPath)}. It should be in an Assets/ subfolder.");
                }

                mod.Logger?.LogError("Please make sure that you uploaded your mod zip correctly.");
                mod.Logger?.LogError("Local mods do not have their folder structure edited, but uploaded mods CAN if you don't follow a specific naming scheme");
                mod.Logger?.LogError(
                    "Here is a working example:\n" +
                    "Folder Structure:\n" +
                    "YOURMOD.zip\n" +
                    "└─ plugins\n" +
                    "    └─ Assets\n" +
                    "        └─ NormalAssetBundlesGoHere\n" +
                    "    └─ DuskModAssetBundleGoesHere\n" +
                    "└─ CHANGELOG.md\n" +
                    "└─ icon.md\n" +
                    "└─ LICENSE.md\n" +
                    "└─ manifest.json\n" +
                    "└─ README.md\n"
                    );
                continue;
            }

            DefaultBundle bundle = new(AssetBundle.LoadFromFile(path))
            {
                AssetBundleData = bundleData
            };
            LoadAllContent(bundle);
        }
    }
}