namespace CodeRebirthLib;
public sealed class CRAchievementInfo : INamespaced<CRAchievementInfo>
{
    internal CRAchievementInfo(NamespacedKey<CRAchievementInfo> key)
    {
        TypedKey = key;
    }
    
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CRAchievementInfo> TypedKey { get; }
}