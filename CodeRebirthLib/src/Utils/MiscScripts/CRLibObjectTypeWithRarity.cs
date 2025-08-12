using System;

namespace CodeRebirthLib;

[Serializable]
public class CRLibObjectTypeWithRarity
{
    public NamespacedKey<CRMapObjectInfo> NamespacedMapObjectKey;
    public int Rarity;
}