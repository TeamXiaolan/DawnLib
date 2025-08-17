using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRMWeatherReference : CRMContentReference<CRMWeatherDefinition, CRWeatherEffectInfo>
{
    public CRMWeatherReference() : base()
    { }

    public CRMWeatherReference(NamespacedKey<CRWeatherEffectInfo> key) : base(key)
    { }

    public override bool TryResolve(out CRWeatherEffectInfo info)
    {
        return LethalContent.Weathers.TryGetValue(TypedKey, out info);
    }

    public override CRWeatherEffectInfo Resolve()
    {
        return LethalContent.Weathers[TypedKey];
    }
}