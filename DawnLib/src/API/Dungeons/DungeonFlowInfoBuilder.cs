using System;
using DunGen.Graph;
using UnityEngine;

namespace Dawn;
public class DungeonFlowInfoBuilder : BaseInfoBuilder<DawnDungeonInfo, DungeonFlow, DungeonFlowInfoBuilder>
{
    private float _mapTileSize = 0f;
    private AudioClip? _firstTimeAudio = null;
    private ProviderTable<int?, DawnMoonInfo>? _weights;
    private string _assetBundlePath = string.Empty;

    internal DungeonFlowInfoBuilder(NamespacedKey<DawnDungeonInfo> key, DungeonFlow value) : base(key, value)
    {
    }

    public DungeonFlowInfoBuilder SetMapTileSize(float mapTileSize)
    {
        _mapTileSize = mapTileSize;
        return this;
    }

    public DungeonFlowInfoBuilder SetAssetBundlePath(string assetBundlePath)
    {
        _assetBundlePath = assetBundlePath;
        return this;
    }

    public DungeonFlowInfoBuilder SetFirstTimeAudio(AudioClip firstTimeAudio)
    {
        _firstTimeAudio = firstTimeAudio;
        return this;
    }

    public DungeonFlowInfoBuilder SetWeights(Action<WeightTableBuilder<DawnMoonInfo>> callback)
    {
        WeightTableBuilder<DawnMoonInfo> builder = new WeightTableBuilder<DawnMoonInfo>();
        callback(builder);
        _weights = builder.Build();
        return this;
    }

    override internal DawnDungeonInfo Build()
    {
        return new DawnDungeonInfo(key, [], value, _weights, _mapTileSize, _firstTimeAudio, _assetBundlePath, customData);
    }
}