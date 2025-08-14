namespace CodeRebirthLib;
public abstract class BaseInfoBuilder<TInfo, T> where TInfo : INamespaced<TInfo>
{
    protected NamespacedKey<TInfo> _key { get; private set; }
    protected T _value { get; private set; }

    internal BaseInfoBuilder(NamespacedKey<TInfo> key, T value)
    {
        _key = key;
        _value = value;
    }

    abstract internal TInfo Build();
}