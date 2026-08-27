using System.Collections;
using System.Collections.Generic;
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

    public override IEnumerator Apply(Transform rootTransform, bool immediate = false)
    {
        if (!immediate)
        {
            yield return null;
        }

        List<Transform> transforms = GetComponentsWithHierarchyPaths<Transform>(rootTransform);
        foreach (Transform transform in transforms)
        {
            if (DeleteGameObject)
            {
                if (transform.gameObject.TryGetComponent(out NetworkObject networkObject) && NetworkManager.Singleton.IsServer)
                {
                    networkObject.Despawn(true);
                }

                if (networkObject == null)
                {
                    Destroy(transform.gameObject);
                }
                yield break;
            }

            if (DisableGameObject)
            {
                transform.gameObject.SetActive(false);
            }
            transform.gameObject.transform.localPosition += PositionOffset;
            transform.gameObject.transform.localRotation *= Quaternion.Euler(RotationOffset);
        }
    }
}