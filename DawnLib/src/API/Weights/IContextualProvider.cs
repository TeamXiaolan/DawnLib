namespace CodeRebirthLib;
public interface IContextualProvider<out T, in TBase> where TBase : INamespaced<TBase>
{
    T Provide(TBase info);
}