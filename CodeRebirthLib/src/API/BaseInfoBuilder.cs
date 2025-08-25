using System.Collections.Generic;

namespace CodeRebirthLib;
public abstract class BaseInfoBuilder<TInfo, T, TBuilder> where TInfo : INamespaced<TInfo> where TBuilder : BaseInfoBuilder<TInfo, T, TBuilder>
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

    public TBuilder AddTags(IEnumerable<NamespacedKey> tags)
    {
        foreach (NamespacedKey tag in this.tags)
        {
            AddTag(tag);
        }
        return (TBuilder)this;
    }

    abstract internal TInfo Build();
}