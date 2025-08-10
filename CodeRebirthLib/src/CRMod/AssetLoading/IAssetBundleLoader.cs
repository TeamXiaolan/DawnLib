
namespace CodeRebirthLib;
public interface IAssetBundleLoader
{
    CRContentDefinition[] Content { get; }
    AssetBundleData? AssetBundleData { get; set; }
}