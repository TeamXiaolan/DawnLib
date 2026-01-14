using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskEnemyReference : DuskContentReference<DuskEnemyDefinition, DawnEnemyInfo>
{
    public DuskEnemyReference() : base()
    { }

    public DuskEnemyReference(NamespacedKey<DawnEnemyInfo> key) : base(key)
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