using UnityEngine;

namespace Dawn;

public class DeadBodyInfoBuilder : BaseInfoBuilder<DawnDeadBodyInfo, GameObject, DeadBodyInfoBuilder>
{
    public DeadBodyInfoBuilder(NamespacedKey<DawnDeadBodyInfo> key, GameObject value) : base(key, value)
    {
    }

    override internal DawnDeadBodyInfo Build()
    {
        return new DawnDeadBodyInfo(key, tags, value, customData);
    }
}