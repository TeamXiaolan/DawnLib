using CodeRebirthLib.Utils;

namespace CodeRebirthLib;

public class AutoWeightTagger(NamespacedKey tag, BoundedRange range) : IAutoTagger<CRItemInfo>
{
    public NamespacedKey Tag => tag;
    public bool ShouldApply(CRItemInfo info)
    {
        return range.IsInRange(info.Item.weight);
    }
}