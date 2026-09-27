using System;
using System.Reflection;
using UnityEngine;

namespace SunSet;

public class AssetBundleLoader<TLoader> where TLoader : AssetBundleLoader<TLoader>
{
    public AssetBundleLoader(AssetBundle bundle)
    {
        Type type = typeof(TLoader);
        foreach (PropertyInfo property in type.GetProperties())
        {
            LoadFromBundleAttribute loadInstruction = (LoadFromBundleAttribute)property.GetCustomAttribute(typeof(LoadFromBundleAttribute));
            if (loadInstruction == null) continue;

            property.SetValue(this, LoadAsset(bundle, loadInstruction.BundleFile));
        }
    }

    private UnityEngine.Object LoadAsset(AssetBundle bundle, string path)
    {
        UnityEngine.Object result = bundle.LoadAsset<UnityEngine.Object>(path);
        if (result == null)
            throw new ArgumentException(path + " is not valid in the assetbundle!");

        return result;
    }
}