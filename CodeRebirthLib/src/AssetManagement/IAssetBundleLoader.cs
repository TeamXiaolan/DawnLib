using CodeRebirthLib.ContentManagement;

namespace CodeRebirthLib.AssetManagement;
public interface IAssetBundleLoader
{
    CRContentDefinition[] Content { get; }
    AssetBundleData? AssetBundleData { get; set; }
}