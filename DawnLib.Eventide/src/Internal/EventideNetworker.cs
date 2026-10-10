using Dawn;
using Dawn.Utils;
using Unity.Netcode;

namespace Eventide.Internal;

public class EventideNetworker : NetworkSingleton<EventideNetworker>
{
    public void Start()
    {
        DawnPlugin.Logger.LogDebug($"{nameof(EventideNetworker)} started.");
    }

    [ServerRpc(RequireOwnership = false)]
    internal void TriggerAchievementServerRpc(NamespacedKey namespacedKey)
    {
        TriggerAchievementClientRpc(namespacedKey);
    }

    [ClientRpc]
    private void TriggerAchievementClientRpc(NamespacedKey namespacedKey)
    {
        EventideModContent.Achievements[namespacedKey.AsTyped<EventideAchievementDefinition>()].TryCompleteFromServer();
    }
}