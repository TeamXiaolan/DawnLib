using Dawn;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New NamespacedObject Definition", menuName = $"{DuskModConstants.Definitions}/NamespacedObject Definition")]
public class DuskNamespacedObjectDefinition : DuskContentDefinition, INamespaced<DuskNamespacedObjectDefinition>
{
    [field: SerializeField]
    private NamespacedKey<DuskNamespacedObjectDefinition> _typedKey;

    [field: SerializeField]
    public GameObject NamespacedObject { get; private set; }

    public NamespacedKey<DuskNamespacedObjectDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<DuskNamespacedObjectDefinition>(); }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        DuskModContent.NamespacedObjects.Register(this);
    }


    public override void TryNetworkRegisterAssets()
    {
        if (!NamespacedObject.TryGetComponent(out NetworkObject _))
        {
            DuskPlugin.Logger.LogWarning($"{NamespacedObject.name} has no NetworkObject, This is not supported for NamespacedObjects, automatically adding one.");
            NamespacedObject.AddComponent<NetworkObject>();
        }

        DawnLib.RegisterNetworkPrefab(NamespacedObject);
    }

    protected override string EntityNameReference => (NamespacedObject != null ? NamespacedObject.name : null) ?? string.Empty;
}