namespace Dawn;
public interface INamespaced
{
    NamespacedKey Key { get; }
}

public interface INamespaced<T> : INamespaced where T : INamespaced<T>
{
    NamespacedKey<T> TypedKey { get; }
}