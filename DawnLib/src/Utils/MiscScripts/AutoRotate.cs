using Dawn.Internal;
using UnityEngine;

namespace Dawn.Utils;

[AddComponentMenu($"{DawnConstants.MiscUtils}/Auto Rotate")]
public class AutoRotate : MonoBehaviour
{
    [SerializeField]
    private float _rotationSpeedMax = 5f;

    [SerializeField]
    private float _rotationSpeedMin = 0f;

    private Vector3 _rotation = Vector3.zero;

    private void Start()
    {
        float _rotationSpeedX = DawnNetworker.Instance!.DawnLibRandom.NextFloat(_rotationSpeedMin, _rotationSpeedMax);
        float _rotationSpeedY = DawnNetworker.Instance!.DawnLibRandom.NextFloat(_rotationSpeedMin, _rotationSpeedMax);
        float _rotationSpeedZ = DawnNetworker.Instance!.DawnLibRandom.NextFloat(_rotationSpeedMin, _rotationSpeedMax);
        _rotation = new Vector3(_rotationSpeedX, _rotationSpeedY, _rotationSpeedZ);
    }

    private void Update()
    {
        transform.Rotate(_rotation * Time.deltaTime);
    }
}