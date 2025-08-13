using CodeRebirthLib.Internal;
using UnityEngine;

namespace CodeRebirthLib.Utils;
public class AutoRotate : MonoBehaviour
{
    [SerializeField]
    private float _rotationSpeedMax = 5f;

    [SerializeField]
    private float _rotationSpeedMin = 0f;

    private Vector3 _rotation = Vector3.zero;

    private void Start()
    {
        float _rotationSpeedX = CodeRebirthLibNetworker.Instance!.CRLibRandom.NextFloat(_rotationSpeedMin, _rotationSpeedMax);
        float _rotationSpeedY = CodeRebirthLibNetworker.Instance!.CRLibRandom.NextFloat(_rotationSpeedMin, _rotationSpeedMax);
        float _rotationSpeedZ = CodeRebirthLibNetworker.Instance!.CRLibRandom.NextFloat(_rotationSpeedMin, _rotationSpeedMax);
        _rotation = new Vector3(_rotationSpeedX, _rotationSpeedY, _rotationSpeedZ);
    }

    private void Update()
    {
        transform.Rotate(_rotation * Time.deltaTime);
    }
}