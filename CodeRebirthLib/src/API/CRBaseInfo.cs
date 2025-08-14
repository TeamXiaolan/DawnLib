namespace CodeRebirthLib;
public abstract class CRBaseInfo<T> : INamespaced<T> where T : CRBaseInfo<T>
{
    protected CRBaseInfo(NamespacedKey<T> key, bool isExternal)
    {
        TypedKey = key;
        IsExternal = isExternal;
    }
    
    internal bool IsExternal { get; }
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<T> TypedKey { get; }
}