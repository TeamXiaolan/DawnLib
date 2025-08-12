using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRWeatherReference : CRContentReference<CRItemDefinition, CRWeatherInfo>
{
    public CRWeatherReference() : base()
    { }
    public CRWeatherReference(NamespacedKey<CRWeatherInfo> key) : base(key)
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