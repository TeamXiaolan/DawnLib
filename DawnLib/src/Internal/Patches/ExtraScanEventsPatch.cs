using Dawn.Utils;
using GameNetcodeStuff;

namespace Dawn.Internal;
static class ExtraScanEventsPatch
{
    internal static void Init()
    {
        On.HUDManager.AttemptScanNode += OnAttemptScanNode;
    }
    private static void OnAttemptScanNode(On.HUDManager.orig_AttemptScanNode orig, HUDManager self, ScanNodeProperties node, int i, PlayerControllerB playerscript)
    {
        orig(self, node, i, playerscript);

        if (self.MeetsScanNodeRequirements(node, playerscript) && node.gameObject.TryGetComponent(out ExtraScanEvents events))
        {
            events._onScan.Invoke();
        }
    }
}