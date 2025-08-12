
namespace CodeRebirthLib.CRMod;
public interface IAssetBundleLoader
{
    CRContentDefinition[] Content { get; }
    AssetBundleData? AssetBundleData { get; set; }
}