using System;

namespace CodeRebirthLib.Utils;

[Serializable]
public class CRLibObjectTypeWithRarity
{
    public NamespacedKey<CRMapObjectInfo> NamespacedMapObjectKey;
    public int Rarity;
}