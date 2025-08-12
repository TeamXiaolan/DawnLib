using System.Collections.Generic;

namespace CodeRebirthLib;
public class ProviderTable<T, TBase> where TBase : INamespaced<TBase>
{
    private List<IProvider<T, TBase>> _providers;
    
    internal ProviderTable(List<IProvider<T, TBase>> providers)
    {
        _providers = providers;
    }

    public T? GetFor(TBase info)
    {
        foreach (IProvider<T?, TBase> provider in _providers)
        {
            T? value = provider.Provide(info);
            if (value == null)
                continue;

            return value;
        }
        
        return default;
    }

    public static ProviderTable<T, TBase> Empty() => new([]);
}