using System;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

namespace Dawn.Compatibility;
class DawnTaggableCondition(Func<ITaggable?> generator) : Condition
{
    public string Value { get; private set; }

    private NamespacedKey? _key;

    public override bool Evaluate(IContext context)
    {
        _key ??= NamespacedKey.ForceParse(Value);

        ITaggable? taggable = generator();
        return taggable != null && taggable.HasTag(_key);
    }
}