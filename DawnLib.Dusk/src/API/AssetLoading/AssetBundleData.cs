using System;

namespace Dusk;

[Serializable]
public class AssetBundleData
{
    [AssetBundleReference]
    public string assetBundleName;

    [AssertNotEmpty]
    public string configName;
}