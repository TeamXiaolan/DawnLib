using System.Collections.Generic;

namespace Dawn;
public class ProviderTable<T, TBase> where TBase : INamespaced<TBase>
{
    private List<IContextualProvider<T, TBase>> _providers;

    internal ProviderTable(List<IContextualProvider<T, TBase>> providers)
    {
        _providers = providers;
    }

    public T? GetFor(TBase info)
    {
        foreach (IContextualProvider<T?, TBase> provider in _providers)
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

public sealed class ProviderTable<T, TBase, TContext> where TBase : INamespaced<TBase>
{
    private readonly List<IContextualProvider<T, TBase, TContext>> _providers;

    internal ProviderTable(List<IContextualProvider<T, TBase, TContext>> providers)
    {
        _providers = providers;
    }

    public T? GetFor(TBase info, in TContext ctx)
    {
        foreach (var provider in _providers)
        {
            T? value = provider.Provide(info, ctx);
            if (value == null)
                continue;

            return value;
        }

        return default;
    }

    public static ProviderTable<T, TBase, TContext> Empty() => new([]);
}
