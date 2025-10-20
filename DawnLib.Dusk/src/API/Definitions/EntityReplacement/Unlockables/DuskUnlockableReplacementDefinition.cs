using System.Collections;
using Dawn.Internal;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

public class DuskUnlockableReplacementDefinition : DuskEntityReplacementDefinition<DuskUnlockable>
{
    public override IEnumerator Apply(DuskUnlockable ai)
    {
        yield break;
    }
}

public abstract class DuskUnlockableReplacementDefinition<T> : DuskUnlockableReplacementDefinition where T : DuskUnlockable
{
    protected abstract void ApplyTyped(T dawnUnlockable);
    public override IEnumerator Apply(DuskUnlockable dawnUnlockable)
    {
        yield return base.Apply(dawnUnlockable);
        dawnUnlockable.SetUnlockableReplacement(this);
        foreach (Hierarchy hierarchyReplacement in Replacements)
        {
            yield return StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(dawnUnlockable.transform));
        }

        foreach (GameObjectWithPath gameObjectAddon in GameObjectAddons)
        {
            if (string.IsNullOrEmpty(gameObjectAddon.PathToGameObject))
            {
                continue;
            }

            GameObject? gameObject = dawnUnlockable.transform.Find(gameObjectAddon.PathToGameObject)?.gameObject;
            if (gameObject != null)
            {
                if (gameObjectAddon.GameObjectToCreate.TryGetComponent(out NetworkObject networkObject) && !NetworkManager.Singleton.IsServer)
                    continue;

                GameObject addOn = GameObject.Instantiate(gameObjectAddon.GameObjectToCreate, gameObject.transform);
                addOn.transform.localPosition = gameObjectAddon.PositionOffset;
                addOn.transform.rotation = Quaternion.Euler(gameObjectAddon.RotationOffset);

                if (networkObject == null)
                    continue;

                networkObject.AutoObjectParentSync = false;
                networkObject.Spawn();
            }
        }
        ApplyTyped((T)dawnUnlockable);
    }
}