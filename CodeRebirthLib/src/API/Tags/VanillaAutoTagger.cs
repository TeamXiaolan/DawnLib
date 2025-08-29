namespace CodeRebirthLib;
public class VanillaAutoTagger<T> : IAutoTagger<T> where T : INamespaced<T>, ITaggable
{

    public NamespacedKey Tag => Tags.Vanilla;
    public bool ShouldApply(T info)
    {
        return info.Key.IsVanilla();
    }
}