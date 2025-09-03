using System;
using UnityEngine;

namespace Dawn;

public class EnemyInfoBuilder : BaseInfoBuilder<CREnemyInfo, EnemyType, EnemyInfoBuilder>
{
    private CREnemyLocationInfo? _inside, _outside, _daytime;
    private TerminalNode? _bestiaryNode;
    private TerminalKeyword? _nameKeyword;
    
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
            if (_weights == null)
            {
                CodeRebirthLibPlugin.Logger.LogWarning($"Enemy '{_parent.key}' didn't set weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
                _weights = ProviderTable<int?, CRMoonInfo>.Empty();
            }
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

    public EnemyInfoBuilder SetBestiaryNode(TerminalNode node)
    {
        _bestiaryNode = node;
        return this;
    }

    public EnemyInfoBuilder OverrideNameKeyword(string word)
    {
        _nameKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
        _nameKeyword.name = word;
        _nameKeyword.word = word;
        return this;
    }
   
    public EnemyInfoBuilder OverrideNameKeyword(TerminalKeyword keyword)
    {
        _nameKeyword = keyword;
        return this;
    }

    override internal CREnemyInfo Build()
    {
        if (_bestiaryNode == null)
        {
            _bestiaryNode = new TerminalNodeBuilder($"{value.enemyName}BestiaryNode")
                .SetDisplayText($"{value.enemyName}\n\nDanger level: Unknown\n\n[No information about this creature was found.]\n\n")
                .SetClearPreviousText(true)
                .SetMaxCharactersToType(35)
                .Build();
        }

        if (_nameKeyword == null)
        {
            string word = value.enemyName.ToLowerInvariant().Replace(' ', '-');
            OverrideNameKeyword(word);
        }

        _bestiaryNode.creatureName = value.enemyName;
        return new CREnemyInfo(key, tags, value, _outside, _inside, _daytime, _bestiaryNode, _nameKeyword!);
    }
}