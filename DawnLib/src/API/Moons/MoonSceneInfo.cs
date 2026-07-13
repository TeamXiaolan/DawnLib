using Dawn.Internal;
using UnityEngine;

namespace Dawn;

public interface IMoonSceneInfo : INamespaced<IMoonSceneInfo>
{
    string SceneName { get; }
    DawnWeightedValue<int> Rarity { get; }
    public int GetRarity(DawnWeatherEffectInfo? weatherEffectInfo = null)
    {
        return Rarity.GetValue(new WeightQuery
        {
            Subject = this,
            Weather = weatherEffectInfo,
            Channel = DawnWeightChannels.ScrapRarity.Key
        });
    }
}

public class VanillaMoonSceneInfo : IMoonSceneInfo
{
    public string SceneName { get; private set; }
    public DawnWeightedValue<int> Rarity { get; private set; } = new DawnWeightedValue<int>(DawnWeightChannels.MoonSceneRarity,
        WeightProfile<int>.Create(DawnWeightChannels.MoonSceneRarity.Policy, weightProfile => weightProfile.AddSource(new GlobalBaseIntSource(() => 100)))); // todo: config?

    internal VanillaMoonSceneInfo(NamespacedKey<IMoonSceneInfo> key, string sceneName)
    {
        SceneName = sceneName;
        TypedKey = key;
    }

    public NamespacedKey Key => TypedKey;
    public NamespacedKey<IMoonSceneInfo> TypedKey { get; }
}

public class CustomMoonSceneInfo : IMoonSceneInfo
{
    public string SceneName { get; }
    public string ScenePath { get; }
    public DawnWeightedValue<int> Rarity { get; private set; }
    public AnimationClip? ShipLandingOverrideAnimation { get; private set; }
    public AnimationClip? ShipTakeoffOverrideAnimation { get; private set; }

    internal string AssetBundlePath;

    public NamespacedKey Key => TypedKey;
    public NamespacedKey<IMoonSceneInfo> TypedKey { get; }

    internal CustomMoonSceneInfo(NamespacedKey<IMoonSceneInfo> key, AnimationClip? shipLandingOverrideAnimation, AnimationClip? shipTakeoffOverrideAnimation, DawnWeightedValue<int> weight, string assetBundlePath, string scenePath)
    {
        AssetBundlePath = assetBundlePath;
        Rarity = weight;
        TypedKey = key;
        ShipLandingOverrideAnimation = shipLandingOverrideAnimation;
        ShipTakeoffOverrideAnimation = shipTakeoffOverrideAnimation;
        ScenePath = scenePath;
        SceneName = DawnNetworkSceneManager.GetSceneNameFromPath(scenePath);
        DawnLib.RegisterNetworkScene(scenePath);
    }
}