using Unity.Netcode.Components;
using UnityEngine;

namespace CodeRebirthLib.MiscScriptManagement;
[DisallowMultipleComponent]
public class OwnerNetworkAnimator : NetworkAnimator // Taken straight from https://docs-multiplayer.unity3d.com/netcode/current/components/networkanimator/
{
    public override bool OnIsServerAuthoritative()
    {
        return false;
    }
}