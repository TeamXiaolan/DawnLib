using Dusk.Internal;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Instant Achievement Definition", menuName = $"{DuskModConstants.Achievements}/Instant Definition")]
[HelpURL("https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/wiki/4107-d1-achievements/")]
public class DuskInstantAchievement : DuskAchievementDefinition
{
    [field: SerializeField]
    public bool SyncedCompletion { get; private set; }

    public bool TriggerAchievement()
    {
        if (SyncedCompletion)
        {
            DuskNetworker.Instance?.TriggerAchievementServerRpc(Key);
        }
        return TryCompleteAchievement();
    }

    public override void TryNetworkRegisterAssets() { }
}
