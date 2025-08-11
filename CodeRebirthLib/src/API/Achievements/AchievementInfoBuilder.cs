using System;

namespace CodeRebirthLib;
public class AchievementInfoBuilder
{
    private NamespacedKey<CRAchievementInfo> _key;
    
    internal AchievementInfoBuilder(NamespacedKey<CRAchievementInfo> key)
    {
        _key = key;
    }

    internal CRAchievementInfo Build()
    {
        return new CRAchievementInfo(_key);
    }
}