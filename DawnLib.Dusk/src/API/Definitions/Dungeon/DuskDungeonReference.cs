using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskDungeonReference : DuskContentReference<DuskDungeonDefinition, DawnDungeonInfo>
{
    public DuskDungeonReference() : base()
    { }
    public DuskDungeonReference(NamespacedKey<DawnDungeonInfo> key) : base(key)
    { }

    public override bool TryResolve(out DawnDungeonInfo info)
    {
        return LethalContent.Dungeons.TryGetValue(TypedKey, out info);
    }

    public override DawnDungeonInfo Resolve()
    {
        return LethalContent.Dungeons[TypedKey];
    }
}