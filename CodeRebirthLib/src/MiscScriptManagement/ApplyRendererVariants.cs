using System;
using System.Collections.Generic;
using CodeRebirthLib.Data;
using CodeRebirthLib.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirth.src.MiscScripts;

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
    public int Weight { get; set; } = 1;
}

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