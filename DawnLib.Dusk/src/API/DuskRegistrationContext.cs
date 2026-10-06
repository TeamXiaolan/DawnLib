namespace Dusk;

public class DuskRegistrationContext
{
    internal DuskRegistrationContext(DuskMod mod, AssetBundleData assetBundleData)
    {
        Mod = mod;
        AssetBundleData = assetBundleData;
    }

    public DuskMod Mod { get; }
    public AssetBundleData AssetBundleData { get; }
}