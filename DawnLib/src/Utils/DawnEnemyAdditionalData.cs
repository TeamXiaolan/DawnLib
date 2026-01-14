using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine;

namespace Dawn.Utils;
public class DawnEnemyAdditionalData : MonoBehaviour
{
    private static readonly Dictionary<EnemyAI, DawnEnemyAdditionalData> _additionalData = new();
    private EnemyAI _enemy;
    private List<EnemyAICollisionDetect> _enemyAICollisionDetects = new();

    public IReadOnlyList<EnemyAICollisionDetect> EnemyAICollisionDetects => _enemyAICollisionDetects.AsReadOnly();
    public PlayerControllerB? PlayerThatLastHit { get; internal set; }
    public bool KilledByPlayer { get; internal set; }

    private void OnDestroy()
    {
        _additionalData.Remove(_enemy);
    }

    public static DawnEnemyAdditionalData CreateOrGet(EnemyAI enemyAI)
    {
        if (_additionalData.TryGetValue(enemyAI, out DawnEnemyAdditionalData data))
        {
            return data;
        }

        data = enemyAI.gameObject.AddComponent<DawnEnemyAdditionalData>();
        data._enemy = enemyAI;
        data._enemyAICollisionDetects = enemyAI.GetComponentsInChildren<EnemyAICollisionDetect>().ToList();
        _additionalData[enemyAI] = data;
        return data;
    }
}