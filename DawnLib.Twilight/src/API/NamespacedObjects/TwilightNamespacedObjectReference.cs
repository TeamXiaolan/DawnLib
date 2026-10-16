using System;
using Dawn;
using Dusk;

namespace Twilight;

[Serializable]
public class TwilightNamespacedObjectReference : DuskContentReference<TwilightNamespacedObjectDefinition, TwilightNamespacedObjectDefinition>
{
    public TwilightNamespacedObjectReference() : base()
    { }

    public TwilightNamespacedObjectReference(NamespacedKey<TwilightNamespacedObjectDefinition> key) : base(key)
    { }

    public override bool TryResolve(out TwilightNamespacedObjectDefinition info)
    {
        return TwilightModContent.NamespacedObjects.TryGetValue(TypedKey, out info);
    }

    public override TwilightNamespacedObjectDefinition Resolve()
    {
        return TwilightModContent.NamespacedObjects[TypedKey];
    }
}