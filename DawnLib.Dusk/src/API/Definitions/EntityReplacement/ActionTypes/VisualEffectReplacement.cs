using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Dusk;

[CreateAssetMenu(fileName = "New VisualEffectAsset Replacement", menuName = $"Entity Replacements/Actions/VisualEffectAsset Replacement")]
public class VisualEffectReplacement : Hierarchy
{
    [field: SerializeField]
    public VisualEffectAsset VisualEffectAssetReplacement { get; private set; }

    public override IEnumerator Apply(Transform rootTransform, bool immediate = false)
    {
        if (!immediate)
        {
            yield return null;
        }

        List<VisualEffect> visualEffects = GetComponentsWithHierarchyPaths<VisualEffect>(rootTransform);
        foreach (VisualEffect visualEffect in visualEffects)
        {
            visualEffect.visualEffectAsset = VisualEffectAssetReplacement;
        }
    }
}

[CreateAssetMenu(fileName = "New VisualEffect Texture Replacement", menuName = $"Entity Replacements/Actions/Texture Replacement")]
public class VisualEffectTextureReplacement : Hierarchy
{
    [field: SerializeField]
    public List<UnknownMap> ReplacementUnknownMaps { get; private set; } = new();

    public override IEnumerator Apply(Transform rootTransform, bool immediate = false)
    {
        if (!immediate)
        {
            yield return null;
        }

        List<VisualEffect> visualEffects = GetComponentsWithHierarchyPaths<VisualEffect>(rootTransform);
        foreach (VisualEffect visualEffect in visualEffects)
        {
            ReplaceTextures(visualEffect);
        }
    }

    private void ReplaceTextures(VisualEffect visualEffect)
    {
        foreach (UnknownMap unknownMap in ReplacementUnknownMaps)
        {
            if (visualEffect.HasTexture(unknownMap.MaskName))
            {
                visualEffect.SetTexture(unknownMap.MaskName, unknownMap.Texture);
            }
        }
    }
}