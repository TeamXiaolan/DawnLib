using Dawn.Preloader;

namespace Dawn.Interfaces;

[InjectInterface("PlaceableShipObject")]
interface IOnDestroyMethod
{
    void OnDestroy();
}