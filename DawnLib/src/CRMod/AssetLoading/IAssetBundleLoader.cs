
namespace CodeRebirthLib.CRMod;
public interface IAssetBundleLoader
{
    CRMContentDefinition[] Content { get; }
    AssetBundleData? AssetBundleData { get; set; }
}