using Unity.Netcode;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New GameObject Editor Action", menuName = $"Entity Replacements/Actions/GameObject Editor Action")]
public class GameObjectEditorAction : Hierarchy
{
    [field: SerializeField]
    public bool DeleteGameObject { get; private set; } = false;

    [field: SerializeField]
    public bool DisableGameObject { get; private set; } = false;

    [field: SerializeField]
    public Vector3 PositionOffset { get; private set; }
    [field: SerializeField]
    public Vector3 RotationOffset { get; private set; }

    public override void Apply(Transform rootTransform)
    {
        GameObject gameObject = !string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).gameObject : rootTransform.gameObject;
        if (DeleteGameObject)
        {
            if (gameObject.TryGetComponent(out NetworkObject networkObject) && NetworkManager.Singleton.IsServer)
            {
                networkObject.Despawn(true);
            }

            if (networkObject == null)
            {
                Destroy(gameObject);
            }
            return;
        }

        if (DisableGameObject)
        {
            gameObject.SetActive(false);
        }
        gameObject.transform.localPosition += PositionOffset;
        gameObject.transform.localRotation *= Quaternion.Euler(RotationOffset);
    }
}