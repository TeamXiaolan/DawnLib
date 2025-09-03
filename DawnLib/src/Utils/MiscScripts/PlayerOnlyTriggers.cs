using GameNetcodeStuff;
using UnityEngine;

namespace CodeRebirthLib.Utils;
public class PlayerOnlyTriggers : MonoBehaviour
{
    public EnemyAI mainScript = null!;

    public void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out PlayerControllerB player))
        {
            mainScript.OnCollideWithPlayer(other);
        }
    }
}