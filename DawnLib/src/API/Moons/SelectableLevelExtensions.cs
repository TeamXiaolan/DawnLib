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
            set => selectableLevel.DawnInfo.OutsideEnemiesProbabilityRange = value;
        }

        public int MaxDaytimeDiversityPowerCount
        {
            get => selectableLevel.DawnInfo.MaxDaytimeDiversityPowerCount;
            set => selectableLevel.DawnInfo.MaxDaytimeDiversityPowerCount = value;
        }

        public int MaxWeedEnemyPowerCount
        {
            get => selectableLevel.DawnInfo.MaxWeedEnemyPowerCount;
            set => selectableLevel.DawnInfo.MaxWeedEnemyPowerCount = value;
        }

        public int MaxWeedDiversityPowerCount
        {
            get => selectableLevel.DawnInfo.MaxWeedDiversityPowerCount;
            set => selectableLevel.DawnInfo.MaxWeedDiversityPowerCount = value;
        }

        public List<SpawnableEnemyWithRarity> WeedEnemies
        {
            get => selectableLevel.DawnInfo.WeedEnemies;
        }

        public AnimationCurve WeedEnemySpawnChanceThroughDay
        {
            get => selectableLevel.DawnInfo.WeedEnemySpawnChanceThroughDay;
            set => selectableLevel.DawnInfo.WeedEnemySpawnChanceThroughDay = value;
        }

        public float WeedEnemiesProbabilityRange
        {
            get => selectableLevel.DawnInfo.WeedEnemiesProbabilityRange;
            set => selectableLevel.DawnInfo.WeedEnemiesProbabilityRange = value;
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