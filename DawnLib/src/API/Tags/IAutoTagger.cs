namespace Dawn;
public interface IAutoTagger<T> where T : INamespaced<T>, ITaggable
{
    public NamespacedKey Tag { get; }
    public bool ShouldApply(T info);
}