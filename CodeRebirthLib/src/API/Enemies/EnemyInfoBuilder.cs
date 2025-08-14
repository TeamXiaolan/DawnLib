using System;
using Unity.Burst;

namespace CodeRebirthLib;

public class EnemyInfoBuilder
{
    private NamespacedKey<CREnemyInfo> _key;
    private EnemyType _enemy;

    private ProviderTable<int?, CRMoonInfo>? _insideWeights, _outsideWeights, _daytimeWeights;

    internal EnemyInfoBuilder(NamespacedKey<CREnemyInfo> key, EnemyType enemy)
    {
        _key = key;
        _enemy = enemy;
    }

    public EnemyInfoBuilder DefineOutside(Action<WeightTableBuilder<CRMoonInfo>> callback)
    {
        WeightTableBuilder<CRMoonInfo> builder = new WeightTableBuilder<CRMoonInfo>();
        callback(builder);
        _outsideWeights = builder.Build();
        return this;
    }

    public EnemyInfoBuilder DefineInside(Action<WeightTableBuilder<CRMoonInfo>> callback)
    {
        WeightTableBuilder<CRMoonInfo> builder = new WeightTableBuilder<CRMoonInfo>();
        callback(builder);
        _insideWeights = builder.Build();
        return this;
    }

    public EnemyInfoBuilder DefineDaytime(Action<WeightTableBuilder<CRMoonInfo>> callback)
    {
        WeightTableBuilder<CRMoonInfo> builder = new WeightTableBuilder<CRMoonInfo>();
        callback(builder);
        _daytimeWeights = builder.Build();
        return this;
    }

    internal CREnemyInfo Build()
    {
        return new CREnemyInfo(_key, false, _enemy, _outsideWeights, _insideWeights, _daytimeWeights);
    }
}