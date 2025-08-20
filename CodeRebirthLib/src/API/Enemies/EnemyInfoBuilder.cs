using System;
using System.Collections.Generic;

namespace CodeRebirthLib;

public class EnemyInfoBuilder
{
    private NamespacedKey<CREnemyInfo> _key;
    private EnemyType _enemyType;

    private CREnemyLocationInfo? _inside, _outside, _daytime;
    private List<NamespacedKey> _tags = new();

    public class EnemyLocationBuilder
    {
        private ProviderTable<int?, CRMoonInfo>? _weights;
        private EnemyInfoBuilder _parent;
        public EnemyLocationBuilder SetWeights(Action<WeightTableBuilder<CRMoonInfo>> callback)
        {
            WeightTableBuilder<CRMoonInfo> builder = new WeightTableBuilder<CRMoonInfo>();
            callback(builder);
            _weights = builder.Build();
            return this;
        }

        internal EnemyLocationBuilder(EnemyInfoBuilder parent)
        {
            _parent = parent;
        }

        internal CREnemyLocationInfo Build()
        {
            return new CREnemyLocationInfo(_weights);
        }
    }
    
    internal EnemyInfoBuilder(NamespacedKey<CREnemyInfo> key, EnemyType enemyType)
    {
        _key = key;
        _enemyType = enemyType;
    }

    public EnemyInfoBuilder DefineOutside(Action<EnemyLocationBuilder> callback)
    {
        EnemyLocationBuilder builder = new EnemyLocationBuilder(this);
        callback(builder);
        _outside = builder.Build();
        return this;
    }
    
    public EnemyInfoBuilder DefineInside(Action<EnemyLocationBuilder> callback)
    {
        EnemyLocationBuilder builder = new EnemyLocationBuilder(this);
        callback(builder);
        _inside = builder.Build();
        return this;
    }
    
    public EnemyInfoBuilder DefineDaytime(Action<EnemyLocationBuilder> callback)
    {
        EnemyLocationBuilder builder = new EnemyLocationBuilder(this);
        callback(builder);
        _daytime = builder.Build();
        return this;
    }

    public EnemyInfoBuilder AddTag(NamespacedKey tag)
    {
        _tags.Add(tag);
        return this;
    }

    internal CREnemyInfo Build()
    {
        return new CREnemyInfo(_key, _tags, _enemyType, _outside, _inside, _daytime);
    }
}