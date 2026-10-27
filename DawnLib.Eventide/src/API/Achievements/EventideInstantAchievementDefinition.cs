using Eventide.Internal;
using UnityEngine;

namespace Eventide;

[CreateAssetMenu(fileName = "New Instant Achievement Definition", menuName = $"{EventideModConstants.Achievements}/Instant Definition")]
public class EventideInstantAchievement : EventideAchievementDefinition
{
    [field: SerializeField]
    public bool SyncedCompletion { get; private set; }

    public bool TriggerAchievement()
    {
        if (SyncedCompletion)
        {
            EventideNetworker.Instance?.TriggerAchievementServerRpc(Key);
        }
        return TryCompleteAchievement();
    }

    public override void TryNetworkRegisterAssets() { }
}
