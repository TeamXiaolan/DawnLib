using System.Linq;
using System.Reflection;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;

namespace CodeRebirthLib.ContentManagement;

public abstract class ContentHandler(CRMod mod)
{
    protected bool IsContentEnabled(AssetBundleData assetBundleData)
    {
        string configName = assetBundleData.configName;

        using(ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(assetBundleData))
        {
            bool isEnabled = section.Bind("Enabled", $"Whether {configName} is enabled.", true).Value;
            return isEnabled;
        }
    }
    
    protected bool TryLoadContentBundle<TAsset>(string assetBundleName, out TAsset? asset) where TAsset : AssetBundleLoader<TAsset>
    {
        asset = null;
        
        AssetBundleData? assetBundleData = mod.Content.assetBundles.FirstOrDefault(it => it.assetBundleName == assetBundleName);
        
        if (assetBundleData == null)
        {
            CodeRebirthLibPlugin.ExtendedLogging($"Plugin with assetbundle name: {assetBundleName} is not implemented yet!");
            return false;
        }

        if (!IsContentEnabled(assetBundleData))
            return false;

        ConstructorInfo constructorInfo = typeof(TAsset).GetConstructor([typeof(CRMod), typeof(string)]);
        asset = (TAsset) constructorInfo.Invoke([mod, assetBundleName]);
        asset.AssetBundleData = assetBundleData;
        
        asset.TryUnload();
        return true;
    }

    protected void LoadAllContent<TAsset>(TAsset bundle) where TAsset : AssetBundleLoader<TAsset>
    {
        CRContentDefinition[] definitions = bundle.Content;
        foreach (CRContentDefinition definition in definitions)
        {
            definition.AssetBundleData = bundle.AssetBundleData;
            definition.Register(mod);
        }
    }

    protected bool RegisterContent<TAsset>(string bundleName, out TAsset? asset) where TAsset : AssetBundleLoader<TAsset>
    {
        if (TryLoadContentBundle(bundleName, out asset))
        {
            LoadAllContent(asset!);
            return true;
        }
        return false;
    }
}

public abstract class ContentHandler<T> : ContentHandler where T : ContentHandler<T>
{
    public static T Instance { get; private set; } = null!;
    
    public ContentHandler(CRMod mod) : base(mod)
    {
        Instance = (T)this;
    }
}