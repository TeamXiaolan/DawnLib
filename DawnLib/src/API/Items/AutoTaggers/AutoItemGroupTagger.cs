using System.Linq;

namespace Dawn;
public class AutoItemGroupTagger(NamespacedKey tag, string itemGroupName) : IAutoTagger<DawnItemInfo>
{

    public NamespacedKey Tag => tag;
    public bool ShouldApply(DawnItemInfo info)
    {
        if (info.Item.spawnPositionTypes == null)
            return false;

        return info.Item.spawnPositionTypes.Any(itemGroup => itemGroup != null && itemGroup.name == itemGroupName);
    }
}