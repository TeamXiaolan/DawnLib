using Unity.Netcode.Components;
using UnityEngine;

namespace Dawn.Utils;
[DisallowMultipleComponent]
[AddComponentMenu($"{DawnConstants.Networking}/Owner Network Animator")]
public class OwnerNetworkAnimator : NetworkAnimator // Taken straight from https://docs-multiplayer.unity3d.com/netcode/current/components/networkanimator/
{
    public override bool OnIsServerAuthoritative()
    {
        return false;
    }
}