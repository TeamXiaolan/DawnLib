using Dawn.Utils;

namespace Dawn;

public class AutoWeightTagger(NamespacedKey tag, BoundedRange range) : IAutoTagger<DawnItemInfo>
{
    public NamespacedKey Tag => tag;
    public bool ShouldApply(DawnItemInfo info)
    {
        return range.IsInRange(info.Item.weight);
    }
}