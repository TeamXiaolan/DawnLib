using System.Collections.Generic;
using Dawn;
using Dawn.Preloader.Interfaces;
using UnityEngine;

namespace Dusk;

public abstract class DuskEntityReplacementDefinition : DuskContentDefinition, INamespaced<DuskEntityReplacementDefinition>
{
    [field: SerializeField, InspectorName("Namespace"), UnlockedNamespacedKey]
    private NamespacedKey<DuskEntityReplacementDefinition> _typedKey;

    [field: SerializeField, InspectorName("Entity to be Replaced"), UnlockedNamespacedKey, Space(5)]
    public NamespacedKey EntityToReplaceKey { get; private set; }

    [field: SerializeField]
    public List<RendererReplacement> RendererReplacements { get; private set; } = new();

    [field: SerializeField]
    public List<GameObjectWithPath> GameObjectAddons { get; private set; } = new(); // TODO if the gameobject has a networkobject, i need to do the finnicky network object parenting stuff? or just disable auto object parent sync

    public NamespacedKey<DuskEntityReplacementDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<DuskEntityReplacementDefinition>(); }
    
    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        DuskModContent.EntityReplacements.Register(this);
    }

    protected override string EntityNameReference => TypedKey.Key;
}

public abstract class DuskEntityReplacementDefinition<TAI> : DuskEntityReplacementDefinition where TAI : class
{
    public virtual void Apply(TAI ai)
    {
        ((ICurrentEntityReplacement)ai).CurrentEntityReplacement = this;
    }
}