using UnityEngine;

namespace CodeRebirthLib.MiscScriptManagement;
public class EnemyOnlyTriggers : MonoBehaviour
{
    public EnemyAI mainScript = null!;

    public void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out EnemyAICollisionDetect enemyAICollisionDetect))
        {
            if (enemyAICollisionDetect.mainScript == mainScript)
                return;

            mainScript.OnCollideWithEnemy(other, enemyAICollisionDetect.mainScript);
        }
    }
}