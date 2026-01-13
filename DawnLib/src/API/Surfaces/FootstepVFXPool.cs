using System.Collections.Generic;
using Dawn.Utils;
using UnityEngine;
using UnityEngine.Pool;

namespace Dawn;

public sealed class FootstepVFXPool : Singleton<FootstepVFXPool>
{
    private readonly Dictionary<int, ObjectPool<PooledFootstepVFX>> _pools = new();

    public void Play(GameObject prefab, Vector3 position, Vector3 normal, Vector3 offset, float scale)
    {
        if (prefab == null)
        {
            return;
        }

        ObjectPool<PooledFootstepVFX> pool = GetOrCreatePool(prefab);
        PooledFootstepVFX footstepVFX = pool.Get();

        Transform footstepVFXTransform = footstepVFX.transform;
        footstepVFXTransform.position = position + offset;

        footstepVFXTransform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

        footstepVFXTransform.localScale = Vector3.one * Mathf.Max(0.0001f, scale);

        footstepVFX.Play(pool);
    }

    private ObjectPool<PooledFootstepVFX> GetOrCreatePool(GameObject prefab)
    {
        int key = prefab.GetInstanceID();

        if (_pools.TryGetValue(key, out ObjectPool<PooledFootstepVFX> existing))
        {
            return existing;
        }

        ObjectPool<PooledFootstepVFX> pool = new ObjectPool<PooledFootstepVFX>(
            createFunc: () =>
            {
                GameObject vfxGameObject = Instantiate(prefab, this.transform);
                PooledFootstepVFX pooled = vfxGameObject.GetComponent<PooledFootstepVFX>() ?? vfxGameObject.AddComponent<PooledFootstepVFX>();
                pooled.Initialize(prefab);
                vfxGameObject.SetActive(false);
                return pooled;
            },
            actionOnGet: vfx =>
            {
                vfx.gameObject.SetActive(true);
            },
            actionOnRelease: vfx =>
            {
                vfx.gameObject.SetActive(false);
                vfx.transform.SetParent(this.transform, false);
            },
            actionOnDestroy: vfx =>
            {
                if (vfx != null)
                {
                    Destroy(vfx.gameObject);
                }
            },
            collectionCheck: false,
            defaultCapacity: 32,
            maxSize: 256
        );

        _pools[key] = pool;
        return pool;
    }
}

public sealed class PooledFootstepVFX : MonoBehaviour
{
    private ParticleSystem[] _systems = null!;
    private ObjectPool<PooledFootstepVFX>? _ownerPool;
    private GameObject _prefab = null!;

    public void Initialize(GameObject prefab)
    {
        _prefab = prefab;
        _systems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void Play(ObjectPool<PooledFootstepVFX> ownerPool)
    {
        _ownerPool = ownerPool;

        for (int i = 0; i < _systems.Length; i++)
        {
            ParticleSystem.MainModule main = _systems[i].main;
            main.stopAction = ParticleSystemStopAction.Callback;
            _systems[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _systems[i].Play(true);
        }
    }

    private void OnParticleSystemStopped()
    {
        for (int i = 0; i < _systems.Length; i++)
        {
            if (_systems[i].IsAlive(true))
            {
                return;
            }
        }

        _ownerPool?.Release(this);
    }
}
