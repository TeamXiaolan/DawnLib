using System.Linq;
using CodeRebirthLib.ContentManagement.Unlockables.Progressive;

namespace CodeRebirthLib.Patches;
static class TerminalPatch
{
    internal static void Init()
    {
        On.Terminal.LoadNewNodeIfAffordable += TerminalOnLoadNewNodeIfAffordable;
    }
    private static void TerminalOnLoadNewNodeIfAffordable(On.Terminal.orig_LoadNewNodeIfAffordable orig, Terminal self, TerminalNode node)
    {
        if (node.shipUnlockableID != -1)
        {
            UnlockableItem unlockableItem = StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID];
            ProgressiveUnlockData? unlockData = ProgressiveUnlockableHandler.AllProgressiveUnlockables
                .FirstOrDefault(it => it.Definition.UnlockableItemDef.unlockable == unlockableItem);

            if (unlockData != null && !unlockData.IsUnlocked)
            {
                orig(self, unlockData.Definition.ProgressiveDenyNode);
                return;
            }
        }
        orig(self, node);
    }
}