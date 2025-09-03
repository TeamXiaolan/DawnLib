
namespace Dawn.Dusk;
public interface IAssetBundleLoader
{
    CRMContentDefinition[] Content { get; }
    AssetBundleData? AssetBundleData { get; set; }
}