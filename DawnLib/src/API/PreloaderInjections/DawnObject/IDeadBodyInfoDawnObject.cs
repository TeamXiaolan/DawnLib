using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(DeadBodyInfo))]
public interface IDeadBodyInfoDawnObject
{
    DawnDeadBodyInfo DawnInfo { get; set; }
}