using System;

namespace Dawn.Utils;

[Serializable]
public class CRLibObjectTypeWithRarity
{
    public NamespacedKey<DawnMapObjectInfo> NamespacedMapObjectKey;
    public int Rarity;
}