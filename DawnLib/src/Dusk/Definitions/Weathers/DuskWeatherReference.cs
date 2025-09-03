using System;

namespace Dawn.Dusk;

[Serializable]
public class DuskWeatherReference : DuskContentReference<DuskWeatherDefinition, DawnWeatherEffectInfo>
{
    public DuskWeatherReference() : base()
    { }

    public DuskWeatherReference(NamespacedKey<DawnWeatherEffectInfo> key) : base(key)
    { }

    public override bool TryResolve(out DawnWeatherEffectInfo info)
    {
        return LethalContent.Weathers.TryGetValue(TypedKey, out info);
    }

    public override DawnWeatherEffectInfo Resolve()
    {
        return LethalContent.Weathers[TypedKey];
    }
}