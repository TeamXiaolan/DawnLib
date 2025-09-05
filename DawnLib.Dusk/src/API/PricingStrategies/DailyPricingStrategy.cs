using Dawn;
using UnityEngine;

namespace Dusk;

// would probably be nice to have as an editor tool the ability to see what the prices would be on each day
// i would have it as like int day1Price, day2Price or smth but there are mods that extend the duration of the quota
[CreateAssetMenu(menuName = $"{DuskModConstants.PricingStrategies}/Daily Pricing Strategy", fileName = "New Daily Pricing", order = DuskModConstants.PricingStrategyOrder)]
public class DailyPricingStrategy : DuskPricingStrategy
{
    [SerializeField, Tooltip("The x axis represents the progress to the deadline. So 1 is company day.")] 
    private AnimationCurve _priceCurve = AnimationCurve.Linear(0, 100, 1, 20);
    
    public override void Register(NamespacedKey id)
    { }
    public override int Provide()
    {
        float totalTime = TimeOfDay.Instance.totalTime * 4f;
        float progress = TimeOfDay.Instance.timeUntilDeadline / totalTime;
        return (int)_priceCurve.Evaluate(1 - progress);
    }
}