using System;

namespace Dawn.Utils;

[Serializable]
public class DawnLibObjectTypeWithRarity
{
    public NamespacedKey<DawnMapObjectInfo> NamespacedMapObjectKey;
    public int Rarity;
}