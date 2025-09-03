using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace Dawn.Utils;

public static class AssetBundleUtils
{
    public static AssetBundle LoadBundle(Assembly assembly, string filePath)
    {
        string correctPath = Path.Combine(Path.GetDirectoryName(assembly.Location), "Assets", filePath);

        if (!File.Exists(correctPath))
        {
            string incorrectPath = Path.Combine(Path.GetDirectoryName(assembly.Location), filePath);
            bool atIncorrectPath = File.Exists(incorrectPath);

            string message = $"The assetbundle at plugins/{Path.GetRelativePath(Paths.PluginPath, correctPath)} does not exist!";
            if (atIncorrectPath)
            {
                message += $" The bundle was found at the incorrect spot: plugins/{Path.GetRelativePath(Paths.PluginPath, incorrectPath)}. It should be within the Assets/ subfolder";
            }

            throw new FileNotFoundException(message);
        }

        return AssetBundle.LoadFromFile(correctPath);
    }
}