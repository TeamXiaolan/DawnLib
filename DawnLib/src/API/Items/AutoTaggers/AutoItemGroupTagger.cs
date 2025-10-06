using System.Linq;

namespace Dawn;
public class AutoItemGroupTagger(NamespacedKey tag, string itemGroupName) : IAutoTagger<DawnItemInfo>
{

    public NamespacedKey Tag => tag;
    public bool ShouldApply(DawnItemInfo info)
    {
        if (info.Item.spawnPositionTypes == null)
            return false;

        foreach (ItemGroup group in info.Item.spawnPositionTypes)
        {
            if (group == null)
            {
                DawnPlugin.Logger.LogWarning($"Item: {info.Key} has a null ItemGroup, why!?!?");
                continue;
            }

            if (group.name == itemGroupName)
            {
                return true;
            }
        }

        return false;
    }
}