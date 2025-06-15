using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace CodeRebirthLib;
public static class CRLib
{
    public static AssetBundle LoadBundle(Assembly assembly, string filePath)
    {
        return AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(assembly.Location), "Assets", filePath));
    }
    
    public static CRMod RegisterMod(BaseUnityPlugin plugin, AssetBundle mainBundle)
    {
        return new CRMod(plugin.GetType().Assembly, plugin, mainBundle);
    }
}