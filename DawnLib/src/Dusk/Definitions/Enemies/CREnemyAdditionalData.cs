using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine;

namespace Dawn;
public class CREnemyAdditionalData : MonoBehaviour
{
    private static readonly Dictionary<EnemyAI, CREnemyAdditionalData> _additionalData = new();
    private EnemyAI _enemy;
    private List<EnemyAICollisionDetect> _enemyAICollisionDetects = new();

    public IReadOnlyList<EnemyAICollisionDetect> EnemyAICollisionDetects => _enemyAICollisionDetects.AsReadOnly();
    public PlayerControllerB? PlayerThatLastHit { get; internal set; }
    public bool KilledByPlayer { get; internal set; }

    private void OnDestroy()
    {
        _additionalData.Remove(_enemy);
    }

    public static CREnemyAdditionalData CreateOrGet(EnemyAI enemyAI)
    {
        if (_additionalData.TryGetValue(enemyAI, out CREnemyAdditionalData data))
        {
            return data;
        }

        data = enemyAI.gameObject.AddComponent<CREnemyAdditionalData>();
        data._enemy = enemyAI;
        data._enemyAICollisionDetects = enemyAI.GetComponentsInChildren<EnemyAICollisionDetect>().ToList();
        _additionalData[enemyAI] = data;
        return data;
    }
}