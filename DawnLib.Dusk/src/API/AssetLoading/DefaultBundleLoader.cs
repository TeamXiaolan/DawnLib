using UnityEngine;

namespace Dusk;

public class DefaultBundleLoader : AssetBundleLoader<DefaultBundleLoader>
{
    internal DefaultBundleLoader(AssetBundle bundle) : base(bundle)
    {
    }
}