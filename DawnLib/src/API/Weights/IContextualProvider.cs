namespace Dawn;
public interface IContextualProvider<out T, in TBase> where TBase : INamespaced<TBase>
{
    T Provide(TBase info);
}

public interface IContextualProvider<out T, in TBase, in TContext> where TBase : INamespaced<TBase>
{
    T Provide(TBase info, TContext ctx);
}