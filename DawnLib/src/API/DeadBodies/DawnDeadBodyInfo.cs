using System.Collections.Generic;
using UnityEngine;

namespace Dawn;

public sealed class DawnDeadBodyInfo : DawnBaseInfo<DawnDeadBodyInfo>
{
    internal DawnDeadBodyInfo(NamespacedKey<DawnDeadBodyInfo> key, HashSet<NamespacedKey> tags, GameObject deadBodyPrefab, IDataContainer? customData) : base(key, tags, customData)
    {
        DeadBodyPrefab = deadBodyPrefab;
    }

    public GameObject DeadBodyPrefab { get; }

    public int Index { get; internal set; }
}