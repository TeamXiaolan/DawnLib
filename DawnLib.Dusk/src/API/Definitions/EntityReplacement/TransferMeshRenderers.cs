using System.Collections.Generic;
using UnityEngine;

namespace Dusk;
public class TransferRenderer : MonoBehaviour
{
    public RendererReplacement RendererReplacement { get; internal set; }

    private void Start()
    {
        if (RendererReplacement == null)
        {
            DuskPlugin.Logger.LogError("TransferMR: RendererReplacement is null.");
            return;
        }

        if (RendererReplacement is SkinnedMeshReplacement skinnedMeshRenderer)
        {
            if (TryGetComponent(out SkinnedMeshRenderer targetSkinned))
            {
                ReplaceSkinnedMeshRenderer(skinnedMeshRenderer, targetSkinned);
                return;
            }
            else
            {
                DuskPlugin.Logger.LogError("TransferMR: Target has no SkinnedMeshRenderer but replacement is SkinnedMeshRenderer.");
                return;
            }
        }

        if (RendererReplacement is MeshReplacement meshReplacement)
        {
            if (TryGetComponent(out MeshRenderer targetMeshRenderer) && TryGetComponent(out MeshFilter targetMeshFilter))
            {
                ReplaceMeshRenderer(meshReplacement, targetMeshRenderer, targetMeshFilter);
                return;
            }
            else
            {
                DuskPlugin.Logger.LogError("TransferMR: Target needs MeshRenderer + MeshFilter to replace from MeshRenderer.");
                return;
            }
        }

        DuskPlugin.Logger.LogError($"TransferMR: Unsupported RendererReplacement type: {RendererReplacement.GetType().Name}");
    }

    private void ReplaceSkinnedMeshRenderer(SkinnedMeshReplacement skinnedMeshReplacement, SkinnedMeshRenderer targetSkinned)
    {
        SkinnedMeshRenderer? skinnedMeshRenderer = skinnedMeshReplacement.ReplacementRenderer;

        if (skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh)
        {
            Transform targetRoot = targetSkinned.rootBone ? targetSkinned.rootBone : transform;
            Dictionary<string, Transform> targetLookup = BuildBoneLookup(targetRoot);

            Transform[] srcBones = skinnedMeshRenderer.bones;
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
            if (skinnedMeshRenderer.rootBone)
            {
                targetLookup.TryGetValue(skinnedMeshRenderer.rootBone.name, out mappedRoot);
            }

            Mesh newMesh = skinnedMeshRenderer.sharedMesh;
            targetSkinned.sharedMesh = newMesh;
            targetSkinned.bones = mappedBones;
            targetSkinned.rootBone = mappedRoot;

            CopyOrResizeMaterials(targetSkinned, skinnedMeshRenderer.sharedMaterials, newMesh ? newMesh.subMeshCount : 1);
        }

        if (skinnedMeshReplacement.ReplacementMaterials != null && skinnedMeshReplacement.ReplacementMaterials.Count > 0)
        {
            Material[] existingMaterials = targetSkinned.sharedMaterials;
            foreach (MaterialWithIndex materialWithIndex in skinnedMeshReplacement.ReplacementMaterials)
            {
                if (materialWithIndex != null && materialWithIndex.Index >= 0 && materialWithIndex.Index < existingMaterials.Length && materialWithIndex.Material != null)
                {
                    existingMaterials[materialWithIndex.Index] = materialWithIndex.Material;
                }
            }
            targetSkinned.sharedMaterials = existingMaterials;
        }
    }

    private void ReplaceMeshRenderer(MeshReplacement meshReplacement, MeshRenderer targetMeshRenderer, MeshFilter targetMeshFilter)
    {
        Mesh? newMesh = meshReplacement.ReplacementMesh;

        if (newMesh)
        {
            targetMeshFilter.sharedMesh = newMesh;
            CopyOrResizeMaterials(targetMeshRenderer, targetMeshRenderer.sharedMaterials, newMesh.subMeshCount);
        }

        if (meshReplacement.ReplacementMaterials != null && meshReplacement.ReplacementMaterials.Count > 0)
        {
            Material[] existingMaterials = targetMeshRenderer.sharedMaterials;
            foreach (MaterialWithIndex materialWithIndex in meshReplacement.ReplacementMaterials)
            {
                if (materialWithIndex != null && materialWithIndex.Index >= 0 && materialWithIndex.Index < existingMaterials.Length && materialWithIndex.Material != null)
                {
                    existingMaterials[materialWithIndex.Index] = materialWithIndex.Material;
                }
            }
            targetMeshRenderer.sharedMaterials = existingMaterials;
        }
    }

    private static void CopyOrResizeMaterials(Renderer target, Material[] sourceMaterials, int requiredCount)
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
        DuskPlugin.Logger.LogWarning($"TransferMR: Material count mismatch (got {got}, need {requiredCount}). Resized with fallback materials.");
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