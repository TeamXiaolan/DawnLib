using System.IO;
using CodeRebirthLib.AssetManagement;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement;
public class DefaultContentHandler : ContentHandler
{
    public DefaultContentHandler(CRMod mod) : base(mod)
    {
        foreach (AssetBundleData bundleData in mod.Content.assetBundles)
        {
            if(!IsContentEnabled(bundleData)) continue;
            DefaultBundle bundle = new DefaultBundle(AssetBundle.LoadFromFile(mod.GetRelativePath("Assets", bundleData.assetBundleName)));
            LoadAllContent(bundle);
        }
    }

}