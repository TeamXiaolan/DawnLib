using System.Collections;
using Dawn.Internal;

namespace Dawn;
public interface MoonSceneInfo : INamespaced<MoonSceneInfo>
{
    string SceneName { get; }
    IProvider<int> Weight { get; }

}

public class VanillaMoonSceneInfo : MoonSceneInfo
{
    public string SceneName { get; private set; }
    public IProvider<int> Weight { get; } = new SimpleProvider<int>(100); // todo: config?

    internal VanillaMoonSceneInfo(NamespacedKey<MoonSceneInfo> key, string sceneName)
    {
        SceneName = sceneName;
        TypedKey = key;
    }
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<MoonSceneInfo> TypedKey { get; }
    public int GetWeight()
    {
        return 100;
    }
}

public class CustomMoonSceneInfo : MoonSceneInfo
{
    public string SceneName { get; }
    public string ScenePath { get; }
    public IProvider<int> Weight { get; } = new SimpleProvider<int>(100);

    internal string AssetBundlePath;
    
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<MoonSceneInfo> TypedKey { get; }

    internal CustomMoonSceneInfo(NamespacedKey<MoonSceneInfo> key, string assetBundlePath, string scenePath)
    {
        AssetBundlePath = assetBundlePath;
        TypedKey = key;
        ScenePath = scenePath;
        SceneName = DawnNetworkSceneManager.GetSceneNameFromPath(scenePath);
        DawnLib.RegisterNetworkScene(scenePath);
    }
}