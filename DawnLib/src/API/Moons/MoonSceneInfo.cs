using Dawn.Internal;

namespace Dawn;
public interface IMoonSceneInfo : INamespaced<IMoonSceneInfo>
{
    string SceneName { get; }
    IProvider<int> Weight { get; }
}

public class VanillaMoonSceneInfo : IMoonSceneInfo
{
    public string SceneName { get; private set; }
    public IProvider<int> Weight { get; private set; } = new SimpleProvider<int>(100); // todo: config?

    internal VanillaMoonSceneInfo(NamespacedKey<IMoonSceneInfo> key, string sceneName)
    {
        SceneName = sceneName;
        TypedKey = key;
    }
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<IMoonSceneInfo> TypedKey { get; }
    public int GetWeight()
    {
        return 100;
    }
}

public class CustomMoonSceneInfo : IMoonSceneInfo
{
    public string SceneName { get; }
    public string ScenePath { get; }
    public IProvider<int> Weight { get; private set; }

    internal string AssetBundlePath;

    public NamespacedKey Key => TypedKey;
    public NamespacedKey<IMoonSceneInfo> TypedKey { get; }

    internal CustomMoonSceneInfo(NamespacedKey<IMoonSceneInfo> key, IProvider<int> weight, string assetBundlePath, string scenePath)
    {
        AssetBundlePath = assetBundlePath;
        Weight = weight;
        TypedKey = key;
        ScenePath = scenePath;
        SceneName = DawnNetworkSceneManager.GetSceneNameFromPath(scenePath);
        DawnLib.RegisterNetworkScene(scenePath);
    }
}