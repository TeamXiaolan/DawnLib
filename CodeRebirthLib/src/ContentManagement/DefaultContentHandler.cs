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

            DefaultBundle bundle = new(AssetBundle.LoadFromFile(mod.GetRelativePath("Assets", bundleData.assetBundleName)));
            bundle.AssetBundleData = bundleData;
            LoadAllContent(bundle);
        }
    }
}