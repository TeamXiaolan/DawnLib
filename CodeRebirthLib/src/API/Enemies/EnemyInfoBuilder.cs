using System;
using System.Collections.Generic;

namespace CodeRebirthLib;

public class EnemyInfoBuilder : BaseInfoBuilder<CREnemyInfo, EnemyType, EnemyInfoBuilder>
{
    private CREnemyLocationInfo? _inside, _outside, _daytime;
    
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
    
    internal EnemyInfoBuilder(NamespacedKey<CREnemyInfo> key, EnemyType enemyType) : base(key, enemyType)
    {
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

    override internal CREnemyInfo Build()
    {
        return new CREnemyInfo(key, tags, value, _outside, _inside, _daytime);
    }
}