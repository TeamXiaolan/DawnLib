using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRMWeatherReference : CRMContentReference<CRMWeatherDefinition, CRWeatherInfo>
{
    public CRMWeatherReference() : base()
    { }

    public CRMWeatherReference(NamespacedKey<CRWeatherInfo> key) : base(key)
    { }

    public override bool TryResolve(out CRWeatherInfo info)
    {
        return LethalContent.Weathers.TryGetValue(TypedKey, out info);
    }

    public override CRWeatherInfo Resolve()
    {
        return LethalContent.Weathers[TypedKey];
    }
}