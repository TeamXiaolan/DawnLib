using UnityEngine;

namespace CodeRebirthLib.AssetManagement;
public class DefaultBundle : AssetBundleLoader<DefaultBundle>
{
    public DefaultBundle(CRMod mod, string filePath) : base(mod, filePath) {
    }
    internal DefaultBundle(AssetBundle bundle) : base(bundle) {
    }
}