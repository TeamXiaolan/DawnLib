using Unity.Netcode;
using UnityEngine;

namespace Dawn.Utils;
[AddComponentMenu($"{DawnConstants.MiscUtils}/StoryLog Spawner")]
public class DawnStoryLogSpawner : MonoBehaviour
{
    [field: SerializeField]
    [field: InspectorName("Namespace")]
    public NamespacedKey NamespacedKey { get; private set; }

    public void Start()
    {
        if (!LethalContent.StoryLogs.TryGetValue(NamespacedKey, out DawnStoryLogInfo storyLogInfo))
        {
            DawnPlugin.Logger.LogWarning($"StoryLog: '{NamespacedKey}' not found to spawn.");
            return;
        }

        if (storyLogInfo.StoryLogGameObject == null)
        {
            DawnPlugin.Logger.LogWarning($"StoryLog: '{NamespacedKey}' has no game object to spawn, this is likely because this is a vanilla story log and zeekerss never made prefabs so it's not possible for DawnLib to grab them... yeah.");
            return;
        }

        GameObject gameObject = GameObject.Instantiate(storyLogInfo.StoryLogGameObject, transform.position, transform.rotation, transform);
        gameObject.GetComponent<NetworkObject>().Spawn(true);
    }
}