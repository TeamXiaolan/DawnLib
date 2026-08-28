using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskNamespacedObjectReference : DuskContentReference<DuskNamespacedObjectDefinition, DuskNamespacedObjectDefinition>
{
    public DuskNamespacedObjectReference() : base()
    { }

    public DuskNamespacedObjectReference(NamespacedKey<DuskNamespacedObjectDefinition> key) : base(key)
    { }

    public override bool TryResolve(out DuskNamespacedObjectDefinition info)
    {
        return DuskModContent.NamespacedObjects.TryGetValue(TypedKey, out info);
    }

    public override DuskNamespacedObjectDefinition Resolve()
    {
        return DuskModContent.NamespacedObjects[TypedKey];
    }
}