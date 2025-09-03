using System;

namespace Dawn.Dusk;

[Serializable]
public class CRMEnemyReference : CRMContentReference<CRMEnemyDefinition, DawnEnemyInfo>
{
    public CRMEnemyReference() : base()
    { }

    public CRMEnemyReference(NamespacedKey<DawnEnemyInfo> key) : base(key)
    { }

    public override bool TryResolve(out DawnEnemyInfo info)
    {
        return LethalContent.Enemies.TryGetValue(TypedKey, out info);
    }

    public override DawnEnemyInfo Resolve()
    {
        return LethalContent.Enemies[TypedKey];
    }
}