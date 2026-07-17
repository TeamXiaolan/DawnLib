using System;
using UnityEngine.Video;

namespace Dawn;

public class EnemyInfoBuilder : BaseInfoBuilder<DawnEnemyInfo, EnemyType, EnemyInfoBuilder>
{
    private DawnEnemyLocationInfo? _inside, _outside, _daytime, _weed;
    private TerminalNode? _bestiaryNode;
    private TerminalKeyword? _nameKeyword;

    public class EnemyLocationBuilder
    {
        private DawnWeightedValue<int>? _weights;
        private EnemyInfoBuilder _parent;
        public EnemyLocationBuilder SetWeights(Action<WeightProfile<int>> callback)
        {
            WeightProfile<int> profile = new WeightProfile<int>(DawnWeightChannels.EnemyRarity.Policy);
            callback(profile);
            _weights = new DawnWeightedValue<int>(DawnWeightChannels.EnemyRarity, profile);
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
                _weights = new DawnWeightedValue<int>(DawnWeightChannels.EnemyRarity);
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

    public EnemyInfoBuilder DefineWeed(Action<EnemyLocationBuilder> callback)
    {
        EnemyLocationBuilder builder = new EnemyLocationBuilder(this);
        callback(builder);
        _weed = builder.Build();
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

    public EnemyInfoBuilder SetBestiaryVideo(VideoClip videoClip)
    {
        if (_bestiaryNode == null)
        {
            throw new InvalidOperationException("Must first call CreateBestiaryNode before setting video.");
        }
        _bestiaryNode.displayVideo = videoClip;

        return this;
    }

    public EnemyInfoBuilder CreateNameKeyword(string wordOverride)
    {
        if (string.IsNullOrWhiteSpace(wordOverride))
        {
            wordOverride = value.enemyName.ToLowerInvariant();
        }

        _nameKeyword = new TerminalKeywordBuilder($"{value.enemyName}NameKeyword", wordOverride, ITerminalKeyword.DawnKeywordType.Bestiary)
            .Build();

        return this;
    }

    override internal DawnEnemyInfo Build()
    {
        _outside ??= new DawnEnemyLocationInfo(new DawnWeightedValue<int>(DawnWeightChannels.EnemyRarity));
        _inside ??= new DawnEnemyLocationInfo(new DawnWeightedValue<int>(DawnWeightChannels.EnemyRarity));
        _daytime ??= new DawnEnemyLocationInfo(new DawnWeightedValue<int>(DawnWeightChannels.EnemyRarity));
        _weed ??= new DawnEnemyLocationInfo(new DawnWeightedValue<int>(DawnWeightChannels.EnemyRarity));

        return new DawnEnemyInfo(key, tags, value, _outside, _inside, _daytime, _weed, _bestiaryNode, _nameKeyword, customData);
    }
}