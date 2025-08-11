namespace CodeRebirthLib;

public class CRWeatherInfo : INamespaced<CRWeatherInfo>
{
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CRWeatherInfo> TypedKey { get; }
}