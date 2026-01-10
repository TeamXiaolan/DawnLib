using Dawn.Interfaces;

namespace Dawn;

public static class SurfaceExtensions
{
    internal static DawnSurfaceInfo GetDawnInfo(this FootstepSurface surface)
    {
        DawnSurfaceInfo surfaceInfo = (DawnSurfaceInfo)((IDawnObject)surface).DawnInfo;
        return surfaceInfo;
    }

    internal static bool HasDawnInfo(this FootstepSurface surface)
    {
        return surface.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this FootstepSurface surface, DawnSurfaceInfo surfaceInfo)
    {
        ((IDawnObject)surface).DawnInfo = surfaceInfo;
    }
}
