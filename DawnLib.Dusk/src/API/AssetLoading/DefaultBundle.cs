using UnityEngine;

namespace Dusk;
public class DefaultBundle : AssetBundleLoader<DefaultBundle>
{
    public DefaultBundle(DuskMod mod, string filePath) : base(mod, filePath)
    {
    }

    internal DefaultBundle(AssetBundle bundle) : base(bundle)
    {
    }
}