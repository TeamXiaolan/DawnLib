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