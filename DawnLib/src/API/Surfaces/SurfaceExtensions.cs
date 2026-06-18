using System;
using Dawn.Interfaces;

namespace Dawn;

public static class FootstepSurfaceExtensions
{
    extension(FootstepSurface footstepSurface)
    {
        public DawnSurfaceInfo DawnInfo
        {
            get => footstepSurface.GetDawnInfoCore();
            set => footstepSurface.SetDawnInfoCore(value);
        }

        [Obsolete("Use FootstepSurface.DawnInfo instead")]
        public DawnSurfaceInfo GetDawnInfo()
        {
            return footstepSurface.GetDawnInfoCore();
        }

        [Obsolete("Use FootstepSurface.DawnInfo instead")]
        public void SetDawnInfo(DawnSurfaceInfo surfaceInfo)
        {
            footstepSurface.SetDawnInfoCore(surfaceInfo);
        }

        [Obsolete]
        public bool HasDawnInfo()
        {
            return footstepSurface.DawnInfo != null;
        }

        private DawnSurfaceInfo GetDawnInfoCore()
        {
            return ((IFootstepSurfaceDawnObject)footstepSurface).DawnInfo;
        }

        private void SetDawnInfoCore(DawnSurfaceInfo surfaceInfo)
        {
            ((IFootstepSurfaceDawnObject)footstepSurface).DawnInfo = surfaceInfo;
        }
    }
}