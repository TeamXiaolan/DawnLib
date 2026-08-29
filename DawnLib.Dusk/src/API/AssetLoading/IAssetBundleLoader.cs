
namespace Dusk;

public interface IAssetBundleLoader
{
    DuskContentDefinition[] Content { get; }
    AssetBundleData AssetBundleData { get; set; }
    DuskConfigRegistry Configs { get; }
}