using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib;

public class EnemyInfoBuilder : BaseInfoBuilder<CREnemyInfo, EnemyType, EnemyInfoBuilder>
{
    private CREnemyLocationInfo? _inside, _outside, _daytime;
    private TerminalNode? _terminalNode;
    private TerminalKeyword? _terminalKeyword;
    
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
        _terminalNode = node;
        return this;
    }

    public EnemyInfoBuilder OverrideNameKeyword(string word)
    {
        _terminalKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
        _terminalKeyword.name = word;
        _terminalKeyword.word = word;
        return this;
    }
   
    public EnemyInfoBuilder OverrideNameKeyword(TerminalKeyword keyword)
    {
        _terminalKeyword = keyword;
        return this;
    }

    override internal CREnemyInfo Build()
    {
        if (_terminalNode == null)
        {
            _terminalNode = new TerminalNodeBuilder($"{value.enemyName}BestiaryNode")
                .SetDisplayText($"{value.enemyName}\n\nDanger level: Unknown\n\n[No information about this creature was found.]\n\n")
                .SetClearPreviousText(true)
                .SetMaxCharactersToType(35)
                .Build();
        }

        if (_terminalKeyword == null)
        {
            string word = value.enemyName.ToLowerInvariant().Replace(' ', '-');
            OverrideNameKeyword(word);
        }

        _terminalNode.creatureName = value.enemyName;
        return new CREnemyInfo(key, tags, value, _outside, _inside, _daytime, _terminalNode, _terminalKeyword);
    }
}