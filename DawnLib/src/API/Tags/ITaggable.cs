using System.Collections.Generic;

namespace Dawn;
public interface ITaggable
{
    bool HasTag(NamespacedKey tag);
    IEnumerable<NamespacedKey> AllTags();
}