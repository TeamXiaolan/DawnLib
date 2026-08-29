using BepInEx.Configuration;

namespace Dusk;

public class DuskRegistrationContext
{
    internal DuskRegistrationContext(DuskMod mod, IAssetBundleLoader bundle)
    {
        Mod = mod;
        Bundle = bundle;
    }

    public DuskMod Mod { get; }

    public IAssetBundleLoader Bundle { get; }

    public AssetBundleData AssetBundleData => Bundle.AssetBundleData;

    internal void RegisterConfig(string name, ConfigEntryBase entry)
    {
        Bundle.Configs.Register(name, entry);
        Mod._configEntries.Add(entry);
    }

    internal void RegisterConfig(ConfigEntryBase entry)
    {
        Mod._configEntries.Add(entry);
    }
}