using System.Collections.Generic;

namespace CodeRebirthLib;
public class CRMoonInfo : CRBaseInfo<CRMoonInfo>
{
    internal CRMoonInfo(NamespacedKey<CRMoonInfo> key, List<NamespacedKey> tags, SelectableLevel level) : base(key, tags)
    {
        Level = level;
    }

    public SelectableLevel Level { get; }
}