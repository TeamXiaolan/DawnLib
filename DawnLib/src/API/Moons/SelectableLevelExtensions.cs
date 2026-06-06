using System;
using System.Collections.Generic;
using Dawn.Interfaces;
using UnityEngine;

namespace Dawn;

public static class SelectableLevelExtensions
{
    extension(SelectableLevel selectableLevel)
    {
        public DawnMoonInfo DawnInfo
        {
            get => selectableLevel.GetDawnInfoCore();
            set => selectableLevel.SetDawnInfoCore(value);
        }

        public float OutsideEnemiesProbabilityRange
        {
            get => selectableLevel.DawnInfo.OutsideEnemiesProbabilityRange;
        }

        public int MaxDaytimeDiversityPowerCount
        {
            get => selectableLevel.DawnInfo.MaxDaytimeDiversityPowerCount;
        }

        public int MaxWeedEnemyPowerCount
        {
            get => selectableLevel.DawnInfo.MaxWeedEnemyPowerCount;
        }

        public int MaxWeedDiversityPowerCount
        {
            get => selectableLevel.DawnInfo.MaxWeedDiversityPowerCount;
        }

        public List<SpawnableEnemyWithRarity> WeedEnemies
        {
            get => selectableLevel.DawnInfo.WeedEnemies;
        }

        public AnimationCurve WeedEnemySpawnChanceThroughDay
        {
            get => selectableLevel.DawnInfo.WeedEnemySpawnChanceThroughDay;
        }

        public float WeedEnemiesProbabilityRange
        {
            get => selectableLevel.DawnInfo.WeedEnemiesProbabilityRange;
        }

        [Obsolete("Use SelectableLevel.DawnInfo instead")]
        public DawnMoonInfo GetDawnInfo()
        {
            return selectableLevel.GetDawnInfoCore();
        }

        private DawnMoonInfo GetDawnInfoCore()
        {
            return ((ISelectableLevelDawnObject)selectableLevel).DawnInfo;
        }

        private void SetDawnInfoCore(DawnMoonInfo selectableLevelInfo)
        {
            ((ISelectableLevelDawnObject)selectableLevel).DawnInfo = selectableLevelInfo;
        }
    }
}