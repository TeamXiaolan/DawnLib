using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Interfaces;
using DunGen.Graph;

namespace Dawn;

public static class EnemyTypeExtensions
{
    extension(EnemyType enemyType)
    {
        public DawnEnemyInfo DawnInfo
        {
            get => enemyType.GetDawnInfoCore();
            set => enemyType.SetDawnInfoCore(value);
        }

        [Obsolete("Use EnemyType.DawnInfo instead")]
        public DawnEnemyInfo GetDawnInfo()
        {
            return enemyType.GetDawnInfoCore();
        }

        private DawnEnemyInfo GetDawnInfoCore()
        {
            return ((IEnemyTypeDawnObject)enemyType).DawnInfo;
        }

        private void SetDawnInfoCore(DawnEnemyInfo enemyTypeInfo)
        {
            ((IEnemyTypeDawnObject)enemyType).DawnInfo = enemyTypeInfo;
        }
    }
}
