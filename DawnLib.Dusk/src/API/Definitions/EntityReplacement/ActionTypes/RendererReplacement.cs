using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Skinned Mesh Replacement", menuName = $"Entity Replacements/Actions/SkinnedMesh Replacement")]
public class SkinnedMeshReplacement : Hierarchy
{
    [field: SerializeField]
    public SkinnedMeshRenderer ReplacementRenderer { get; private set; }

    public override IEnumerator Apply(Transform rootTransform, bool immediate = false)
    {
        if (!immediate)
        {
            yield return null;
        }

        List<SkinnedMeshRenderer> skinnedMeshRenderers = GetComponentsWithHierarchyPaths<SkinnedMeshRenderer>(rootTransform);
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            ReplaceSkinnedMeshRenderer(skinnedMeshRenderer);
        }
    }

    private void ReplaceSkinnedMeshRenderer(SkinnedMeshRenderer targetSkinned)
    {
        Material[] originalMaterials = targetSkinned.sharedMaterials;

        Transform targetRoot = targetSkinned.rootBone;
        Dictionary<string, Transform> targetLookup = BuildBoneLookup(targetRoot);

        Transform[] srcBones = ReplacementRenderer.bones;
        Transform[] mappedBones = new Transform[srcBones.Length];
        for (int i = 0; i < srcBones.Length; i++)
        {
            string? name = srcBones[i] ? srcBones[i].name : null;
            if (string.IsNullOrWhiteSpace(name) || !targetLookup.TryGetValue(name, out Transform transform))
            {
                DuskPlugin.Logger.LogWarning($"TransferSMR: Could not map bone '{name}' with replacement: {ReplacementRenderer.name}. Using root fallback.");
                transform = targetRoot;
            }
            mappedBones[i] = transform;
        }

        Transform mappedRoot = targetSkinned.rootBone ? targetSkinned.rootBone : targetRoot;
        if (ReplacementRenderer.rootBone)
        {
            targetLookup.TryGetValue(ReplacementRenderer.rootBone.name, out mappedRoot);
        }

        Mesh newMesh = ReplacementRenderer.sharedMesh;
        targetSkinned.sharedMesh = newMesh;
        targetSkinned.bones = mappedBones;
        targetSkinned.rootBone = mappedRoot;

        MaterialsReplacement.CopyOrResizeMaterials(targetSkinned, originalMaterials, newMesh ? newMesh.subMeshCount : 1);
    }

    private static Dictionary<string, Transform> BuildBoneLookup(Transform root)
    {
        Dictionary<string, Transform> dict = new();
        if (!root)
        {
            return dict;
        }

        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (!dict.ContainsKey(transform.name))
            {
                dict.Add(transform.name, transform);
            }
        }

        return dict;
    }
}

[CreateAssetMenu(fileName = "New Mesh Replacement", menuName = $"Entity Replacements/Actions/Mesh Replacement")]
public class MeshReplacement : Hierarchy
{
    [field: SerializeField]
    public Mesh ReplacementMesh { get; private set; }

    public override IEnumerator Apply(Transform rootTransform, bool immediate = false)
    {
        if (!immediate)
        {
            yield return null;
        }

        List<MeshRenderer> meshRenderers = GetComponentsWithHierarchyPaths<MeshRenderer>(rootTransform);
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            ReplaceMeshRenderer(meshRenderer, meshRenderer.GetComponent<MeshFilter>());
        }
    }

    private void ReplaceMeshRenderer(MeshRenderer targetMeshRenderer, MeshFilter targetMeshFilter)
    {
        targetMeshFilter.sharedMesh = ReplacementMesh;
        MaterialsReplacement.CopyOrResizeMaterials(targetMeshRenderer, targetMeshRenderer.sharedMaterials, ReplacementMesh.subMeshCount);
    }
}

[CreateAssetMenu(fileName = "New Material Replacement", menuName = $"Entity Replacements/Actions/Material Replacement")]
public class MaterialsReplacement : Hierarchy
{
    [field: SerializeField]
    public List<MaterialWithIndex> ReplacementMaterials { get; private set; } = new();

    public override IEnumerator Apply(Transform rootTransform, bool immediate = false)
    {
        if (!immediate)
        {
            yield return null;
        }

        List<Renderer> renderers = GetComponentsWithHierarchyPaths<Renderer>(rootTransform);
        foreach (Renderer renderer in renderers)
        {
            ReplaceMaterials(renderer);
        }
    }

    private void ReplaceMaterials(Renderer targetRenderer)
    {
        Material[] existingMaterials = targetRenderer.sharedMaterials;
        foreach (MaterialWithIndex materialWithIndex in ReplacementMaterials)
        {
            if (materialWithIndex != null && materialWithIndex.Index >= 0 && materialWithIndex.Index < existingMaterials.Length)
            {
                existingMaterials[materialWithIndex.Index] = materialWithIndex.Material;
            }
        }
        targetRenderer.sharedMaterials = existingMaterials;
    }

    internal static void CopyOrResizeMaterials(Renderer target, Material[] sourceMaterials, int requiredCount)
    {
        if (sourceMaterials != null && sourceMaterials.Length == requiredCount)
        {
            target.sharedMaterials = sourceMaterials;
            return;
        }

        Material[] resized = new Material[Mathf.Max(1, requiredCount)];
        Material[] targetExisting = target.sharedMaterials;

        for (int i = 0; i < resized.Length; i++)
        {
            if (sourceMaterials != null && i < sourceMaterials.Length && sourceMaterials[i] != null)
            {
                resized[i] = sourceMaterials[i];
            }
            else if (sourceMaterials != null && sourceMaterials.Length > 0 && sourceMaterials[0] != null)
            {
                resized[i] = sourceMaterials[0];
            }
            else if (targetExisting != null && targetExisting.Length > 0 && targetExisting[0] != null)
            {
                resized[i] = targetExisting[0];
            }
            else
            {
                resized[i] = new Material(Shader.Find("HDRP/Lit"));
            }
        }

        target.sharedMaterials = resized;

        int got = sourceMaterials?.Length ?? 0;
        DuskPlugin.Logger.LogWarning($"TransferRenderer: Material count mismatch (got {got}, need {requiredCount}). Resized with fallback materials.");
    }
}

[CreateAssetMenu(fileName = "New MaterialProperties Replacement", menuName = $"Entity Replacements/Actions/MaterialProperties Replacement")]
public class TextureReplacement : Hierarchy
{
    [field: SerializeField]
    public List<MaterialPropertiesWithIndex> ReplacementMaterialProperties { get; private set; } = new();

    private static readonly int _mainTex = Shader.PropertyToID("_MainTex");
    private static readonly int _diffuse = Shader.PropertyToID("_Diffuse");
    private static readonly int _maskMap = Shader.PropertyToID("_MaskMap");
    private static readonly int _normalMap = Shader.PropertyToID("_NormalMap");
    private static readonly int _gradient_color = Shader.PropertyToID("_Gradient_Color");

    public override IEnumerator Apply(Transform rootTransform, bool immediate = false)
    {
        if (!immediate)
        {
            yield return null;
        }

        List<Renderer> renderers = GetComponentsWithHierarchyPaths<Renderer>(rootTransform);
        foreach (Renderer renderer in renderers)
        {
            ReplaceTextures(renderer);
        }
    }

    private void ReplaceTextures(Renderer targetRenderer)
    {
        Material[] existingMaterials = targetRenderer.materials;
        foreach (MaterialPropertiesWithIndex materialPropertyWithIndex in ReplacementMaterialProperties)
        {
            if (materialPropertyWithIndex.Index < existingMaterials.Length)
            {
                foreach (UnknownMap unknownMap in materialPropertyWithIndex.UnknownMaps)
                {
                    if (existingMaterials[materialPropertyWithIndex.Index].HasTexture(unknownMap.MaskName))
                    {
                        existingMaterials[materialPropertyWithIndex.Index].SetTexture(unknownMap.MaskName, unknownMap.Texture);
                    }
                }
                if (materialPropertyWithIndex.BaseMap != null && existingMaterials[materialPropertyWithIndex.Index].HasTexture(_mainTex))
                {
                    existingMaterials[materialPropertyWithIndex.Index].mainTexture = materialPropertyWithIndex.BaseMap;
                }
                if (materialPropertyWithIndex.DiffuseMap != null && existingMaterials[materialPropertyWithIndex.Index].HasTexture(_diffuse))
                {
                    existingMaterials[materialPropertyWithIndex.Index].SetTexture(_diffuse, materialPropertyWithIndex.DiffuseMap);
                }
                if (materialPropertyWithIndex.MaskMap != null && existingMaterials[materialPropertyWithIndex.Index].HasTexture(_maskMap))
                {
                    existingMaterials[materialPropertyWithIndex.Index].SetTexture(_maskMap, materialPropertyWithIndex.MaskMap);
                }
                if (materialPropertyWithIndex.NormalMap != null && existingMaterials[materialPropertyWithIndex.Index].HasTexture(_normalMap))
                {
                    existingMaterials[materialPropertyWithIndex.Index].SetTexture(_normalMap, materialPropertyWithIndex.NormalMap);
                }
                if (materialPropertyWithIndex.GradientColor != Color.black && existingMaterials[materialPropertyWithIndex.Index].HasColor(_gradient_color))
                {
                    existingMaterials[materialPropertyWithIndex.Index].SetColor(_gradient_color, materialPropertyWithIndex.GradientColor);
                }
            }
        }
        targetRenderer.materials = existingMaterials;
    }
}

[Serializable]
public class MaterialPropertiesWithIndex
{
    [field: SerializeField]
    public Texture2D? BaseMap { get; private set; }
    [field: SerializeField]
    public Texture2D? DiffuseMap { get; private set; }
    [field: SerializeField]
    public Texture2D? MaskMap { get; private set; }
    [field: SerializeField]
    public Texture2D? NormalMap { get; private set; }
    [field: SerializeField]
    public List<UnknownMap> UnknownMaps { get; private set; } = new();

    [field: Tooltip("I think only hydrogere would make use of this?")]
    [field: SerializeField]
    public Color GradientColor { get; private set; } = Color.black;

    [field: SerializeField]
    [field: Min(0)]
    public int Index { get; private set; }
}

[Serializable]
public class UnknownMap
{
    [field: SerializeField]
    public Texture2D? Texture { get; private set; }
    [field: SerializeField]
    public string MaskName { get; private set; }
}