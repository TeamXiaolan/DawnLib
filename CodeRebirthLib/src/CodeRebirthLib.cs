using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using CodeRebirthLib.ContentManagement;
using CodeRebirthLib.Extensions;
using UnityEngine;

namespace CodeRebirthLib;
public static class CodeRebirthLib
{
    public static AssetBundle LoadBundle(Assembly assembly, string filePath)
    {
        return AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(assembly.Location)!, "Assets", filePath));
    }
    
    public static CRMod RegisterMod(BaseUnityPlugin plugin, AssetBundle mainBundle)
    {
        return new CRMod(plugin.GetType().Assembly, plugin, mainBundle);
    }
}