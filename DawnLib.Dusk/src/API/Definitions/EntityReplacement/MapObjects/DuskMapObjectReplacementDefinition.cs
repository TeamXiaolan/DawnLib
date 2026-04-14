using System.Collections;
using Dawn.Internal;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

public class DuskMapObjectReplacementDefinition : DuskEntityReplacementDefinition<DuskMapObject>
{
    public override IEnumerator Apply(DuskMapObject ai, bool immediate = false)
    {
        yield break;
    }
}

public abstract class DuskMapObjectReplacementDefinition<T> : DuskMapObjectReplacementDefinition where T : DuskMapObject
{
    protected abstract void ApplyTyped(T dawnMapObject);
    public override IEnumerator Apply(DuskMapObject dawnMapObject, bool immediate = false)
    {
        yield return base.Apply(dawnMapObject);
        dawnMapObject.SetMapObjectReplacement(this);
        foreach (Hierarchy hierarchyReplacement in Replacements)
        {
            if (immediate)
            {
                StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(dawnMapObject.transform, immediate));
            }
            else
            {
                yield return StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(dawnMapObject.transform, immediate));
            }
        }

        foreach (GameObjectWithPath gameObjectAddon in GameObjectAddons)
        {
            if (string.IsNullOrWhiteSpace(gameObjectAddon.PathToGameObject))
            {
                continue;
            }

            GameObject? gameObject = dawnMapObject.transform.Find(gameObjectAddon.PathToGameObject)?.gameObject;
            if (gameObject != null)
            {
                if (gameObjectAddon.GameObjectToCreate.TryGetComponent(out NetworkObject networkObject) && !NetworkManager.Singleton.IsServer)
                    continue;

                GameObject addOn = GameObject.Instantiate(gameObjectAddon.GameObjectToCreate, gameObject.transform);
                addOn.transform.localPosition = gameObjectAddon.PositionOffset;
                addOn.transform.localRotation = Quaternion.Euler(gameObjectAddon.RotationOffset);

                if (networkObject == null)
                    continue;

                networkObject.AutoObjectParentSync = false;
                networkObject.Spawn();
            }
        }
        ApplyTyped((T)dawnMapObject);
    }
}