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
    }
}