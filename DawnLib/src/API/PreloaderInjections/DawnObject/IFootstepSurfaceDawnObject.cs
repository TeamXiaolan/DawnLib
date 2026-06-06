using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(FootstepSurface))]
public interface IFootstepSurfaceDawnObject
{
    DawnSurfaceInfo DawnInfo { get; set; }
}