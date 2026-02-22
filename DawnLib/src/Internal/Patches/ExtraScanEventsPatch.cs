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
        bool wasContained = self.scanNodes.ContainsValue(node);
        orig(self, node, i, playerscript);
        if (!wasContained && self.scanNodes.ContainsValue(node) && self.MeetsScanNodeRequirements(node, playerscript) && node.gameObject.TryGetComponent(out ExtraScanEvents events))
        {
            events.OnScan();
        }
    }
}