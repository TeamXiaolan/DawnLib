using GameNetcodeStuff;
using UnityEngine;

namespace CodeRebirthLib.Util;
public class ExtraEnemyData : MonoBehaviour
{
    [HideInInspector]
    public EnemyAI enemyAI = null!;

    [HideInInspector]
    public PlayerControllerB? playerThatLastHit = null;

    [HideInInspector]
    public bool enemyKilledByPlayer = false;

    [HideInInspector]
    public EnemyAICollisionDetect[] enemyAICollisionDetects = [];

    public void Start()
    {
        enemyAICollisionDetects = enemyAI.gameObject.GetComponentsInChildren<EnemyAICollisionDetect>(true);
    }
}