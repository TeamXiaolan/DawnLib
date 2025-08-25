using System.Collections.Generic;

namespace CodeRebirthLib;
public abstract class BaseInfoBuilder
{
    // todo: better name!?!?
    internal abstract void SoloAddTags(IEnumerable<NamespacedKey> newTags);
}

public abstract class BaseInfoBuilder<TInfo, T, TBuilder> : BaseInfoBuilder where TInfo : INamespaced<TInfo> where TBuilder : BaseInfoBuilder<TInfo, T, TBuilder>
{
    protected NamespacedKey<TInfo> key { get; private set; }
    protected T value { get; private set; }
    protected List<NamespacedKey> tags = new();

    internal BaseInfoBuilder(NamespacedKey<TInfo> key, T value)
    {
        this.key = key;
        this.value = value;
    }
    
    public TBuilder AddTag(NamespacedKey tag)
    {
        tags.Add(tag);
        return (TBuilder)this;
    }

    public TBuilder AddTags(IEnumerable<NamespacedKey> newTags)
    {
        foreach (NamespacedKey tag in newTags)
        {
            AddTag(tag);
        }
        return (TBuilder)this;
    }

    override internal void SoloAddTags(IEnumerable<NamespacedKey> newTags)
    {
        AddTags(newTags);
    }

    abstract internal TInfo Build();
}