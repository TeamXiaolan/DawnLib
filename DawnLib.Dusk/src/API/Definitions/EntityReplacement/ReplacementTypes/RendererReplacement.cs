using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Skinned Mesh Replacement", menuName = $"Entity Replacements/Replacements/SkinnedMesh Replacement")]
public class SkinnedMeshReplacement : HierarchyReplacement
{
    [field: SerializeField]
    public SkinnedMeshRenderer ReplacementRenderer { get; private set; }

    public override void Apply(Transform rootTransform)
    {
        ReplaceSkinnedMeshRenderer(!string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).GetComponent<SkinnedMeshRenderer>() : rootTransform.GetComponent<SkinnedMeshRenderer>());
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
            if (string.IsNullOrEmpty(name) || !targetLookup.TryGetValue(name, out Transform transform))
            {
                DuskPlugin.Logger.LogWarning($"TransferSMR: Could not map bone '{name}'. Using root fallback.");
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

[CreateAssetMenu(fileName = "New Mesh Replacement", menuName = $"Entity Replacements/Replacements/Mesh Replacement")]
public class MeshReplacement : HierarchyReplacement
{
    [field: SerializeField]
    public Mesh ReplacementMesh { get; private set; }

    public override void Apply(Transform rootTransform)
    {
        ReplaceMeshRenderer(!string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).GetComponent<MeshRenderer>() : rootTransform.GetComponent<MeshRenderer>(), !string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).GetComponent<MeshFilter>() : rootTransform.GetComponent<MeshFilter>());
    }

    private void ReplaceMeshRenderer(MeshRenderer targetMeshRenderer, MeshFilter targetMeshFilter)
    {
        targetMeshFilter.sharedMesh = ReplacementMesh;
        MaterialsReplacement.CopyOrResizeMaterials(targetMeshRenderer, targetMeshRenderer.sharedMaterials, ReplacementMesh.subMeshCount);
    }
}

[CreateAssetMenu(fileName = "New Material Replacement", menuName = $"Entity Replacements/Replacements/Material Replacement")]
public class MaterialsReplacement : HierarchyReplacement
{
    [field: SerializeField]
    public List<MaterialWithIndex> ReplacementMaterials { get; private set; } = new();

    public override void Apply(Transform rootTransform)
    {
        ReplaceMaterials(!string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).GetComponent<Renderer>() : rootTransform.GetComponent<Renderer>());
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

[CreateAssetMenu(fileName = "New MaterialProperties Replacement", menuName = $"Entity Replacements/Replacements/MaterialProperties Replacement")]
public class TextureReplacement : HierarchyReplacement
{
    [field: SerializeField]
    public List<MaterialPropertiesWithIndex> ReplacementMaterialProperties { get; private set; } = new();

    public override void Apply(Transform rootTransform)
    {
        ReplaceMaterials(!string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).GetComponent<Renderer>() : rootTransform.GetComponent<Renderer>());
    }

    private void ReplaceMaterials(Renderer targetRenderer)
    {
        Material[] existingMaterials = targetRenderer.materials;
        foreach (MaterialPropertiesWithIndex materialPropertyWithIndex in ReplacementMaterialProperties)
        {
            if (materialPropertyWithIndex != null && materialPropertyWithIndex.Index >= 0 && materialPropertyWithIndex.Index < existingMaterials.Length)
            {
                if (materialPropertyWithIndex.BaseMap != null && existingMaterials[materialPropertyWithIndex.Index].HasTexture("_MainTex"))
                {
                    existingMaterials[materialPropertyWithIndex.Index].mainTexture = materialPropertyWithIndex.BaseMap;
                }
                if (materialPropertyWithIndex.MaskMap != null && existingMaterials[materialPropertyWithIndex.Index].HasTexture("_MaskMap"))
                {
                    existingMaterials[materialPropertyWithIndex.Index].SetTexture("_MaskMap", materialPropertyWithIndex.MaskMap);
                }
                if (materialPropertyWithIndex.NormalMap != null && existingMaterials[materialPropertyWithIndex.Index].HasTexture("_NormalMap"))
                {
                    existingMaterials[materialPropertyWithIndex.Index].SetTexture("_NormalMap", materialPropertyWithIndex.NormalMap);
                }
                if (materialPropertyWithIndex.GradientColor != Color.black && existingMaterials[materialPropertyWithIndex.Index].HasColor("_Gradient_Color"))
                {
                    existingMaterials[materialPropertyWithIndex.Index].SetColor("_Gradient_Color", materialPropertyWithIndex.GradientColor);
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
    public Texture2D? MaskMap { get; private set; }
    [field: SerializeField]
    public Texture2D? NormalMap { get; private set; }

    [field: Tooltip("I think only hydrogere would make use of this?")]
    [field: SerializeField]
    public Color GradientColor { get; private set; } = Color.black;

    [field: SerializeField]
    public int Index { get; private set; }
}