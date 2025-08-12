using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRItemReference : CRContentReference<CRItemDefinition, CRItemInfo>
{
    public CRItemReference() : base()
    { }
    public CRItemReference(NamespacedKey<CRItemInfo> key) : base(key) 
    { }

    public override bool TryResolve(out CRItemInfo info)
    {
        return LethalContent.Items.TryGetValue(TypedKey, out info);
    }
}