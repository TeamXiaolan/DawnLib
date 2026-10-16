using Dawn;
using Dusk;
using Unity.Netcode;
using UnityEngine;

namespace Twilight;

[CreateAssetMenu(fileName = "New NamespacedObject Definition", menuName = $"{TwilightModConstants.Definitions}/NamespacedObject Definition")]
public class TwilightNamespacedObjectDefinition : DuskContentDefinition, INamespaced<TwilightNamespacedObjectDefinition>
{
    [field: SerializeField]
    private NamespacedKey<TwilightNamespacedObjectDefinition> _typedKey;

    [field: SerializeField]
    public GameObject NamespacedObject { get; private set; }

    public NamespacedKey<TwilightNamespacedObjectDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<TwilightNamespacedObjectDefinition>(); }

    public override void Register(DuskRegistrationContext registrationContext)
    {
        base.Register(registrationContext);
        TwilightModContent.NamespacedObjects.Register(this);
    }

    public override void TryNetworkRegisterAssets()
    {
        if (!NamespacedObject.TryGetComponent(out NetworkObject _))
        {
            TwilightPlugin.Logger.LogWarning($"{NamespacedObject.name} has no NetworkObject, This is not supported for NamespacedObjects, automatically adding one.");
            NamespacedObject.AddComponent<NetworkObject>();
        }

        DawnLib.RegisterNetworkPrefab(NamespacedObject);
    }

    protected override string EntityNameReference => (NamespacedObject != null ? NamespacedObject.name : null) ?? string.Empty;
}