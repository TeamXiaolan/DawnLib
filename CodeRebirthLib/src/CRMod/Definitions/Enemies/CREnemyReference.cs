using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CREnemyReference : CRContentReference<CRItemDefinition, CREnemyInfo>
{
    public CREnemyReference() : base()
    { }
    public CREnemyReference(NamespacedKey<CREnemyInfo> key) : base(key)
    { }
    public override bool TryResolve(out CREnemyInfo info)
    {
        return LethalContent.Enemies.TryGetValue(TypedKey, out info);
    }

    public override CREnemyInfo Resolve()
    {
        return LethalContent.Enemies[TypedKey];
    }
}