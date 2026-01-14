using System;
using Unity.Netcode;
using UnityEngine;

namespace Dawn.Utils;

[Serializable]
public class MaterialsWithRenderer
{
    public Material[] materials = [];
    public Renderer renderer = null!;
}

[Serializable]
public class MaterialRendererVariantWithWeight : IWeighted
{
    public MaterialsWithRenderer materialsWithRenderer = new();

    [field: SerializeField]
    int _weight { get; set; } = 1;

    public int GetWeight() => _weight;
}

[AddComponentMenu($"{DawnConstants.MiscUtils}/Apply Renderer Variants")]
public class ApplyRendererVariants : NetworkBehaviour
{
    [SerializeField]
    private MaterialRendererVariantWithWeight[] _materialsWithRendererWithWeight = [];

    [SerializeField]
    private bool _applyOnSpawn = true;

    private NetworkVariable<int> _currentVariant = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            System.Random random = new System.Random();
            _currentVariant.Value = Array.IndexOf(_materialsWithRendererWithWeight, random.NextWeighted(_materialsWithRendererWithWeight));
        }

        if (!_applyOnSpawn)
            return;

        ApplyMaterialVariantsToRenderers();
    }

    public void ApplyMaterialVariantsToRenderers()
    {
        _materialsWithRendererWithWeight[_currentVariant.Value].materialsWithRenderer.renderer.sharedMaterials = _materialsWithRendererWithWeight[_currentVariant.Value].materialsWithRenderer.materials;
    }
}