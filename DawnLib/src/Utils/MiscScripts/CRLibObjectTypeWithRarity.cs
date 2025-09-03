using System;

namespace Dawn.Utils;

[Serializable]
public class CRLibObjectTypeWithRarity
{
    public NamespacedKey<CRMapObjectInfo> NamespacedMapObjectKey;
    public int Rarity;
}