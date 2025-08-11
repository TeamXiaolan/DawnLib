namespace CodeRebirthLib;
public interface IProvider<out T, in TBase> where TBase : INamespaced<TBase>
{
    T Provide(TBase info);
}