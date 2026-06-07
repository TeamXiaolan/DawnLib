using Dawn.Interfaces;

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

        private int GetCurrentDaytimeDiversityCore()
        {
            return ((IRoundManagerInjects)roundManager).CurrentDaytimeDiversity;
        }

        private void SetCurrentDaytimeDiversityCore(int value)
        {
            ((IRoundManagerInjects)roundManager).CurrentDaytimeDiversity = value;
        }

        public int CurrentMaxDaytimeDiversityLevel
        {
            get => roundManager.GetCurrentMaxDaytimeDiversityCore();
            set => roundManager.SetCurrentMaxDaytimeDiversityCore(value);
        }

        private int GetCurrentMaxDaytimeDiversityCore()
        {
            return ((IRoundManagerInjects)roundManager).CurrentDaytimeMaxDiversity;
        }

        private void SetCurrentMaxDaytimeDiversityCore(int value)
        {
            ((IRoundManagerInjects)roundManager).CurrentDaytimeMaxDiversity = value;
        }

        public float CurrentMaxDaytimePower
        {
            get => roundManager.GetCurrentMaxDaytimePowerCore();
            set => roundManager.SetCurrentMaxDaytimePowerCore(value);
        }

        private float GetCurrentMaxDaytimePowerCore()
        {
            return ((IRoundManagerInjects)roundManager).CurrentDaytimeMaxPower;
        }

        private void SetCurrentMaxDaytimePowerCore(float value)
        {
            ((IRoundManagerInjects)roundManager).CurrentDaytimeMaxPower = value;
        }

        public int CurrentWeedEnemyDiversityLevel
        {
            get => roundManager.GetCurrentWeedDiversityCore();
            set => roundManager.SetCurrentWeedDiversityCore(value);
        }

        private int GetCurrentWeedDiversityCore()
        {
            return ((IRoundManagerInjects)roundManager).CurrentWeedDiversity;
        }

        private void SetCurrentWeedDiversityCore(int value)
        {
            ((IRoundManagerInjects)roundManager).CurrentWeedDiversity = value;
        }

        public int CurrentMaxWeedDiversityLevel
        {
            get => roundManager.GetCurrentMaxWeedDiversityCore();
            set => roundManager.SetCurrentMaxWeedDiversityCore(value);
        }

        private int GetCurrentMaxWeedDiversityCore()
        {
            return ((IRoundManagerInjects)roundManager).CurrentWeedMaxDiversity;
        }

        private void SetCurrentMaxWeedDiversityCore(int value)
        {
            ((IRoundManagerInjects)roundManager).CurrentWeedMaxDiversity = value;
        }

        public float CurrentMaxWeedPower
        {
            get => roundManager.GetCurrentMaxWeedPowerCore();
            set => roundManager.SetCurrentMaxWeedPowerCore(value);
        }

        private float GetCurrentMaxWeedPowerCore()
        {
            return ((IRoundManagerInjects)roundManager).CurrentWeedMaxPower;
        }

        private void SetCurrentMaxWeedPowerCore(float value)
        {
            ((IRoundManagerInjects)roundManager).CurrentWeedMaxPower = value;
        }
    }
}