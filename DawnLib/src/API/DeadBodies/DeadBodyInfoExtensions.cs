using Dawn.Interfaces;

namespace Dawn;

public static class DeadBodyInfoExtensions
{
    extension(DeadBodyInfo deadBody)
    {
        public DawnDeadBodyInfo DawnInfo
        {
            get => deadBody.GetDawnInfoCore();
            set => deadBody.SetDawnInfoCore(value);
        }

        private DawnDeadBodyInfo GetDawnInfoCore()
        {
            return ((IDeadBodyInfoDawnObject)deadBody).DawnInfo;
        }

        private void SetDawnInfoCore(DawnDeadBodyInfo dawnBodyInfo)
        {
            ((IDeadBodyInfoDawnObject)deadBody).DawnInfo = dawnBodyInfo;
        }
    }
}
