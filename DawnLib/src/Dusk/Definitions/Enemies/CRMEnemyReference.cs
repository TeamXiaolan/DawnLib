using System;

namespace Dawn.Dusk;

[Serializable]
public class CRMEnemyReference : CRMContentReference<CRMEnemyDefinition, CREnemyInfo>
{
    public CRMEnemyReference() : base()
    { }

    public CRMEnemyReference(NamespacedKey<CREnemyInfo> key) : base(key)
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