using System.Collections.Generic;

namespace CodeRebirthLib;
public interface ITaggable
{
    bool HasTag(NamespacedKey tag);
    IEnumerable<NamespacedKey> AllTags();
}