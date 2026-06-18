using System;
using Dawn.Interfaces;
using DunGen.Graph;

namespace Dawn;

public static class DungeonFlowExtensions
{
    extension(DungeonFlow dungeonFlow)
    {
        public DawnDungeonInfo DawnInfo
        {
            get => dungeonFlow.GetDawnInfoCore();
            set => dungeonFlow.SetDawnInfoCore(value);
        }

        [Obsolete("Use DungeonFlow.DawnInfo instead")]
        public DawnDungeonInfo GetDawnInfo()
        {
            return dungeonFlow.GetDawnInfoCore();
        }

        [Obsolete("Use DungeonFlow.DawnInfo instead")]
        public void SetDawnInfo(DawnDungeonInfo dungeonFlowInfo)
        {
            dungeonFlow.SetDawnInfoCore(dungeonFlowInfo);
        }

        [Obsolete]
        public bool HasDawnInfo()
        {
            return dungeonFlow.DawnInfo != null;
        }

        private DawnDungeonInfo GetDawnInfoCore()
        {
            return ((IDunGenFlowDawnObject)dungeonFlow).DawnInfo;
        }

        private void SetDawnInfoCore(DawnDungeonInfo dungeonFlowInfo)
        {
            ((IDunGenFlowDawnObject)dungeonFlow).DawnInfo = dungeonFlowInfo;
        }
    }
}
