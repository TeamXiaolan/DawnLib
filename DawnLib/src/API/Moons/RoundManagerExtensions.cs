using System;
using System.Collections.Generic;
using Dawn.Interfaces;
using UnityEngine;

namespace Dawn;

public static class RoundManagerExtensions
{
    extension(RoundManager roundManager)
    {
        public int CurrentDaytimeEnemyDiversityLevel
        {
            get => roundManager.GetCurrentDaytimeDiversityCore();
            set => roundManager.SetCurrentDaytimeDiversityCore(value);
        }

        public int CurrentMaxDaytimeDiversityLevel
        {
            get => roundManager.GetCurrentMaxDaytimeDiversityCore();
            set => roundManager.SetCurrentMaxDaytimeDiversityCore(value);
        }

        private int GetCurrentDaytimeDiversityCore()
        {
            return ((IRoundManagerInjects)roundManager).CurrentDaytimeDiversity;
        }

        private void SetCurrentDaytimeDiversityCore(int value)
        {
            ((IRoundManagerInjects)roundManager).CurrentDaytimeDiversity = value;
        }

        private int GetCurrentMaxDaytimeDiversityCore()
        {
            return ((IRoundManagerInjects)roundManager).CurrentDaytimeMaxDiversity;
        }

        private void SetCurrentMaxDaytimeDiversityCore(int value)
        {
            ((IRoundManagerInjects)roundManager).CurrentDaytimeMaxDiversity = value;
        }
    }
}