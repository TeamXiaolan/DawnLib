using System.Collections.Generic;
using Dawn;
using UnityEngine;

namespace Dusk;

public abstract class DuskEntityReplacementDefinition : DuskContentDefinition, INamespaced<DuskEntityReplacementDefinition>
{
    public const string REGISTRY_ID = "entityreplacements";

    [field: SerializeField]
    private NamespacedKey<DuskEntityReplacementDefinition> _typedKey;

    [field: SerializeField]
    public NamespacedKey EntityToReplaceKey { get; private set; }

    public NamespacedKey<DuskEntityReplacementDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<DuskEntityReplacementDefinition>(); }

    public List<StringWithAudioClip> AudioClipToReplaceWithFieldNames { get; private set; } = new();
    public List<StringWithAudioClipArray> AudioClipToReplaceWithFieldNamesArray { get; private set; } = new();
    public List<StringWithAudioClipList> AudioClipToReplaceWithFieldNamesList { get; private set; } = new();

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        DuskModContent.EntityReplacements.Register(this);
    }

    protected override string EntityNameReference => TypedKey.Key;
}