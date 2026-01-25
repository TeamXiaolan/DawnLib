using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Dawn.Utils;

[AddComponentMenu($"{DawnConstants.MoonUtils}/Foggy Override")]
public class DawnFoggyOverride : MonoBehaviour
{
    [field: SerializeField]
    public Vector3 NewFogSize { get; private set; } = new Vector3(50f, 15f, 50f);

    private LocalVolumetricFog _fog;
    private Vector3 _oldSize;
    public void Awake()
    {
        GameObject permanentEffectObject = LethalContent.Weathers[WeatherKeys.Foggy].WeatherEffect.effectPermanentObject;
        _fog = permanentEffectObject.GetComponent<LocalVolumetricFog>();

        _oldSize = _fog.parameters.size;
        _fog.parameters.size = NewFogSize;
    }

    public void OnDestroy()
    {
        _fog.parameters.size = _oldSize;
    }
}