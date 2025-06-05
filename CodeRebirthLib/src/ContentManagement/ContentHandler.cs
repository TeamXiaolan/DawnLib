using System;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;

namespace CodeRebirthLib.ContentManagement;

public abstract class ContentHandler<T> where T : ContentHandler<T>
{
    public static T Instance { get; private set; } = null!;

    private CRMod _mod;
    
    public ContentHandler(CRMod mod)
    {
        Instance = (T)this;
        _mod = mod;
    }

    protected bool TryLoadContentBundle<TAsset>(string assetBundleName, out TAsset? asset) where TAsset : AssetBundleLoader<TAsset>
    {
        asset = null;
        
        AssetBundleData? assetBundleData = _mod.Content.assetBundles.FirstOrDefault(it => it.assetBundleName == assetBundleName);
        
        if (assetBundleData == null)
        {
            CodeRebirthLibPlugin.ExtendedLogging($"Plugin with assetbundle name: {assetBundleName} is not implemented yet!");
            return false;
        }
        string configName = assetBundleData.configName;

        using(ConfigContext section = _mod.ConfigManager.CreateConfigSectionForBundleData(assetBundleData))
        {
            bool isEnabled = section.Bind("Enabled", $"Whether {configName} is enabled.", true).Value;
            if (!isEnabled)
            {
                return false;
            }
        }

        ConstructorInfo constructorInfo = typeof(TAsset).GetConstructor([typeof(CRMod), typeof(string)]);
        asset = (TAsset) constructorInfo.Invoke([_mod, assetBundleName]);
        asset.AssetBundleData = assetBundleData;

        return true;
    }

    protected void LoadAllContent<TAsset>(TAsset bundle) where TAsset : AssetBundleLoader<TAsset>
    {
        CRContentDefinition[] definitions = bundle.Content;
        foreach (CRContentDefinition definition in definitions)
        {
            definition.Register(_mod);
        }
    }
}