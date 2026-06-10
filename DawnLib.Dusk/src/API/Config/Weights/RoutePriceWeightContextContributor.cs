using Dawn;

namespace Dusk.Weights;

public sealed class RoutePriceWeightContextContributor : IWeightContextContributor
{
    public void Contribute(WeightContextBuilder builder)
    {
        DawnMoonInfo? moon = builder.Query.Moon;

        if (moon == null)
            return;

        builder.Set(DuskWeightContextKeys.RoutePrice, moon.DawnPurchaseInfo.Cost.Provide());
    }
}