namespace Dawn;
public class CustomAutoTagger<T> : IAutoTagger<T> where T : INamespaced<T>, ITaggable
{
    public NamespacedKey Tag => Tags.Custom;
    public bool ShouldApply(T info)
    {
        return !info.Key.IsVanilla();
    }
}