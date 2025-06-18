using System.Reflection;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;

namespace CodeRebirthLib.ContentManagement;
public abstract class ContentHandler(CRMod mod)
{
    protected bool IsContentEnabled(string bundleName)
    {
        if (!mod.TryGetBundleDataFromName(bundleName, out AssetBundleData? data))
        {
            return false;
        }
        return IsContentEnabled(data);
    }

    protected bool IsContentEnabled(AssetBundleData assetBundleData)
    {
        string configName = assetBundleData.configName;

        using (ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(assetBundleData))
        {
            bool isEnabled = section.Bind("Enabled", $"Whether {configName} is enabled.", true).Value;
            return isEnabled;
        }
    }

    protected bool TryLoadContentBundle<TAsset>(string assetBundleName, out TAsset? asset, bool forceEnabled = false) where TAsset : AssetBundleLoader<TAsset>
    {
        asset = null;

        if (!mod.TryGetBundleDataFromName(assetBundleName, out AssetBundleData? assetBundleData))
        {
            mod.Logger?.LogWarning($"Assetbundle name: {assetBundleName} is not implemented yet!");
            return false;
        }

        if (!forceEnabled && !IsContentEnabled(assetBundleData))
            return false;

        ConstructorInfo constructorInfo = typeof(TAsset).GetConstructor([typeof(CRMod), typeof(string)]);
        asset = (TAsset)constructorInfo.Invoke([mod, assetBundleName]);
        asset.AssetBundleData = assetBundleData;

        asset.TryUnload();
        return true;
    }

    protected void LoadAllContent(IAssetBundleLoader bundle)
    {
        CRContentDefinition[] definitions = bundle.Content;
        foreach (CRContentDefinition definition in definitions)
        {
            definition.AssetBundleData = bundle.AssetBundleData;
            definition.Register(mod);
        }
    }

    protected void LoadAllContent(params IAssetBundleLoader[] bundles)
    {
        foreach (IAssetBundleLoader bundle in bundles)
        {
            LoadAllContent(bundle);
        }
    }

    protected bool RegisterContent<TAsset>(string bundleName, out TAsset? asset, bool forceEnabled = false) where TAsset : AssetBundleLoader<TAsset>
    {
        if (TryLoadContentBundle(bundleName, out asset, forceEnabled))
        {
            LoadAllContent(asset!);
            return true;
        }
        return false;
    }

    protected bool RegisterContentByRef<TAsset>(string bundleName, ref TAsset? asset, bool forceEnabled = false) where TAsset : AssetBundleLoader<TAsset>
    {
        if (TryLoadContentBundle(bundleName, out asset, forceEnabled))
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