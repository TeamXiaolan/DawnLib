namespace CodeRebirthLib.AssetManagement;
public class DefaultBundle : AssetBundleLoader<DefaultBundle>
{
    protected DefaultBundle(CRMod mod, string filePath) : base(mod, filePath) {
    }
}