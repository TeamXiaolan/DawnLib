namespace CodeRebirthLib.AssetManagement;
public class DefaultBundleLoader : AssetBundleLoader<DefaultBundleLoader>
{
    protected DefaultBundleLoader(CRMod mod, string filePath) : base(mod, filePath) {
    }
}