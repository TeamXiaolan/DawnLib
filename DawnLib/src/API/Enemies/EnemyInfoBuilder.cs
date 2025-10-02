using System;
using UnityEngine;

namespace Dawn;

public class EnemyInfoBuilder : BaseInfoBuilder<DawnEnemyInfo, EnemyType, EnemyInfoBuilder>
{
    private DawnEnemyLocationInfo? _inside, _outside, _daytime;
    private TerminalNode? _bestiaryNode;
    private TerminalKeyword? _nameKeyword;

    public class EnemyLocationBuilder
    {
        private ProviderTable<int?, DawnMoonInfo>? _weights;
        private EnemyInfoBuilder _parent;
        public EnemyLocationBuilder SetWeights(Action<WeightTableBuilder<DawnMoonInfo>> callback)
        {
            WeightTableBuilder<DawnMoonInfo> builder = new WeightTableBuilder<DawnMoonInfo>();
            callback(builder);
            _weights = builder.Build();
            return this;
        }

        internal EnemyLocationBuilder(EnemyInfoBuilder parent)
        {
            _parent = parent;
        }

        internal DawnEnemyLocationInfo Build()
        {
            if (_weights == null)
            {
                DawnPlugin.Logger.LogWarning($"Enemy '{_parent.key}' didn't set weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
                _weights = ProviderTable<int?, DawnMoonInfo>.Empty();
            }
            return new DawnEnemyLocationInfo(_weights);
        }
    }

    internal EnemyInfoBuilder(NamespacedKey<DawnEnemyInfo> key, EnemyType enemyType) : base(key, enemyType)
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

    public EnemyInfoBuilder CreateBestiaryNode(string bestiaryNodeText)
    {
        _bestiaryNode = new TerminalNodeBuilder($"{value.enemyName}BestiaryNode")
            .SetDisplayText(bestiaryNodeText)
            .SetCreatureName(value.enemyName)
            .SetClearPreviousText(true)
            .SetMaxCharactersToType(35)
            .Build();

        return this;
    }

    public EnemyInfoBuilder CreateNameKeyword(string wordOverride)
    {
        if (string.IsNullOrEmpty(wordOverride))
        {
            wordOverride = value.enemyName.ToLowerInvariant().Replace(' ', '-');
        }

        _nameKeyword = new TerminalKeywordBuilder($"{value.enemyName}NameKeyword")
            .SetWord(wordOverride)
            .Build();

        return this;
    }

    override internal DawnEnemyInfo Build()
    {
        return new DawnEnemyInfo(key, tags, value, _outside, _inside, _daytime, _bestiaryNode, _nameKeyword, customData);
    }
}