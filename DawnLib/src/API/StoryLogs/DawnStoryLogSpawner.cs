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

        GameObject gameObject = GameObject.Instantiate(storyLogInfo.StoryLogGameObject, transform.position, transform.rotation, transform);
        gameObject.GetComponent<NetworkObject>().Spawn(true);
    }
}