using System;

namespace Dawn;
public class SimpleAutoTagger<T>(NamespacedKey tag, Func<T, bool> predicate) : IAutoTagger<T> where T : CRBaseInfo<T>
{

    public NamespacedKey Tag => tag;
    public bool ShouldApply(T info)
    {
        return predicate(info);
    }
}