namespace Dawn;
public class AllAutoTagger<T> : IAutoTagger<T> where T : INamespaced<T>, ITaggable
{
    public NamespacedKey Tag => Tags.All;
    public bool ShouldApply(T info)
    {
        return true;
    }
}