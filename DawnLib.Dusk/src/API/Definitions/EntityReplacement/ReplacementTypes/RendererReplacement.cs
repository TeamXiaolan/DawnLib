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
        ReplaceSkinnedMeshRenderer(rootTransform.Find(HierarchyPath).GetComponent<SkinnedMeshRenderer>());
    }

    private void ReplaceSkinnedMeshRenderer(SkinnedMeshRenderer targetSkinned)
    {
        if (ReplacementRenderer.sharedMesh)
        {
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

            MaterialsReplacement.CopyOrResizeMaterials(targetSkinned, ReplacementRenderer.sharedMaterials, newMesh ? newMesh.subMeshCount : 1);
        }
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
        ReplaceMeshRenderer(rootTransform.Find(HierarchyPath).GetComponent<MeshRenderer>(), rootTransform.Find(HierarchyPath).GetComponent<MeshFilter>());
    }

    private void ReplaceMeshRenderer(MeshRenderer targetMeshRenderer, MeshFilter targetMeshFilter)
    {
        targetMeshFilter.sharedMesh = ReplacementMesh;
        MaterialsReplacement.CopyOrResizeMaterials(targetMeshRenderer, targetMeshRenderer.sharedMaterials, ReplacementMesh.subMeshCount);
    }
}

[CreateAssetMenu(fileName = "New Materials Replacement", menuName = $"Entity Replacements/Replacements/Material Replacement")]
public class MaterialsReplacement : HierarchyReplacement
{
    [field: SerializeField]
    public List<MaterialWithIndex> ReplacementMaterials { get; private set; } = new();

    public override void Apply(Transform rootTransform)
    {
        ReplaceMaterials(rootTransform.Find(HierarchyPath).GetComponent<Renderer>());
    }

    private void ReplaceMaterials(Renderer targetRenderer)
    {
        Material[] existingMaterials = targetRenderer.sharedMaterials;
        foreach (MaterialWithIndex materialWithIndex in ReplacementMaterials)
        {
            if (materialWithIndex != null && materialWithIndex.Index >= 0 && materialWithIndex.Index < existingMaterials.Length && materialWithIndex.Material != null)
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