using System.Reflection;
using System.Runtime.CompilerServices;

namespace CodeRebirthLib.CRMod;
public abstract class ContentHandler(CRMod mod)
{
    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    protected bool IsContentEnabled(string bundleName)
    {
        if (!mod.TryGetBundleDataFromName(bundleName, out AssetBundleData? data))
        {
            return false;
        }
        return IsContentEnabled(data);
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
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

        bool isEnabled = IsContentEnabled(assetBundleData);
        if (!isEnabled && !forceEnabled)
            return false;

        ConstructorInfo? constructorInfo = typeof(TAsset).GetConstructor([typeof(CRMod), typeof(string)]);

        if (constructorInfo == null)
        {
            mod.Logger?.LogError($"{typeof(TAsset).Name} is not properly setup to handle TryLoadContentBundle. It must have a constructor with (CRMod, string) as arguments!");
            return false;
        }

        asset = (TAsset)constructorInfo.Invoke([mod, assetBundleName]);
        asset.AssetBundleData = assetBundleData;

        asset.TryUnload();
        return true;
    }

    protected void LoadAllContent(IAssetBundleLoader bundle)
    {
        CRMContentDefinition[] definitions = bundle.Content;
        foreach (CRMContentDefinition definition in definitions)
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
}

public abstract class ContentHandler<T> : ContentHandler where T : ContentHandler<T>
{
    public ContentHandler(CRMod mod) : base(mod)
    {
        Instance = (T)this;
    }
    public static T Instance { get; private set; } = null!;
}