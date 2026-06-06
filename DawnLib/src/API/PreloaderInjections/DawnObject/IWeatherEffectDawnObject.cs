using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(WeatherEffect))]
public interface IWeatherEffectDawnObject
{
    DawnWeatherEffectInfo DawnInfo { get; set; }
}