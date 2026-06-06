using System;
using Dawn.Interfaces;

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