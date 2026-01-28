using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Dawn.Utils;
[AddComponentMenu($"{DawnConstants.MoonUtils}/Dust Clouds Override")]
public class DawnDustCloudsOverride : MonoBehaviour
{
    [field: SerializeField]
    public Vector3 NewFogSize { get; private set; } = new Vector3(50f, 15f, 50f);

    private LocalVolumetricFog _fog;
    private Vector3 _oldSize;
    public void Awake()
    {
        GameObject effectObject = LethalContent.Weathers[WeatherKeys.Rollinggroundfog].WeatherEffect.effectObject;
        _fog = effectObject.GetComponent<LocalVolumetricFog>();

        _oldSize = _fog.parameters.size;
        _fog.parameters.size = NewFogSize;
    }

    public void OnDestroy()
    {
        _fog.parameters.size = _oldSize;
    }
}