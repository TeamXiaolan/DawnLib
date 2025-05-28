using CodeRebirthLib.AssetManagement;
using UnityEngine;
using UnityEngine.Serialization;
using WeatherRegistry;

namespace CodeRebirthLib.ContentManagement.Weathers;
[CreateAssetMenu(fileName = "New Weather Definition", menuName = "CodeRebirthLib/Weather Definition")]
public class CRWeatherDefinition : CRContentDefinition
{
    [field: FormerlySerializedAs("Weather"), SerializeField]
    public Weather Weather { get; private set; }
}