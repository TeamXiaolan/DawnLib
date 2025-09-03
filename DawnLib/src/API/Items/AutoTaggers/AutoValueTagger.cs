using CodeRebirthLib.Utils;

namespace CodeRebirthLib;

public class AutoValueTagger(NamespacedKey tag, BoundedRange range) : IAutoTagger<CRItemInfo>
{
    public NamespacedKey Tag => tag;
    public bool ShouldApply(CRItemInfo info)
    {
        float averageValue = (info.Item.maxValue + info.Item.minValue) / 2f;
        return range.IsInRange(averageValue);
    }
}