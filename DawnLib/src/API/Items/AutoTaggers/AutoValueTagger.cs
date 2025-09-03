using Dawn.Utils;

namespace Dawn;

public class AutoValueTagger(NamespacedKey tag, BoundedRange range) : IAutoTagger<DawnItemInfo>
{
    public NamespacedKey Tag => tag;
    public bool ShouldApply(DawnItemInfo info)
    {
        float averageValue = (info.Item.maxValue + info.Item.minValue) / 2f;
        return range.IsInRange(averageValue);
    }
}