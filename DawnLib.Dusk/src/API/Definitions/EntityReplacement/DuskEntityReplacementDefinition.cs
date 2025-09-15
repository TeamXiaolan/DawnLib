using Dawn;
using UnityEngine;

namespace Dusk;

public abstract class DuskEntityReplacementDefinition : DuskContentDefinition, INamespaced<DuskEntityReplacementDefinition>
{
    public const string REGISTRY_ID = "entityreplacements";

    [field: SerializeField, InspectorName("Namespace")]
    private NamespacedKey<DuskEntityReplacementDefinition> _typedKey;

    [field: SerializeField]
    public string EntityReplacementName { get; private set; }

    [field: SerializeField, InspectorName("Entity to be Replaced"), UnlockedNamespacedKey, Space(5)]
    public NamespacedKey EntityToReplaceKey { get; private set; }

    public NamespacedKey<DuskEntityReplacementDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<DuskEntityReplacementDefinition>(); }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        DuskModContent.EntityReplacements.Register(this);
    }

    protected override string EntityNameReference => EntityReplacementName;
}

public abstract class DuskEntityReplacementDefinition<TAI> : DuskEntityReplacementDefinition where TAI : class
{
    public abstract void Apply(TAI ai);
}