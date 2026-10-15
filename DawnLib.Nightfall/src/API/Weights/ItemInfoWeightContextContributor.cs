using Dawn;

namespace Nightfall.Weights;

public sealed class ItemInfoWeightContextContributor : IWeightContextContributor
{
    public void Contribute(WeightContextBuilder builder)
    {
        object? owner = builder.Query.Owner;
        if (owner == null || owner is not DawnItemInfo itemInfo)
            return;

        builder.Set(NightfallWeightContextKeys.ItemInfo, itemInfo);
    }
}