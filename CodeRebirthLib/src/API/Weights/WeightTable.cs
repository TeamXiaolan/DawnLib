using System.Collections.Generic;

namespace CodeRebirthLib;
public class WeightTable<TBase> where TBase : INamespaced<TBase>
{
    private List<IWeightProvider<TBase>> _providers;
    
    internal WeightTable(List<IWeightProvider<TBase>> providers)
    {
        _providers = providers;
    }

    public int GetWeightFor(TBase info)
    {
        foreach (IWeightProvider<TBase> provider in _providers)
        {
            if(!provider.IsActive(info)) continue;
            return provider.GetWeight(info);
        }
        
        return 0;
    }

    public static WeightTable<TBase> Empty() => new WeightTable<TBase>([]);
}