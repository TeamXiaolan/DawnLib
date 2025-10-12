using UnityEngine;
using UnityEngine.VFX;

namespace Dusk;

[CreateAssetMenu(fileName = "New VisualEffectAsset Replacement", menuName = $"Entity Replacements/Actions/VisualEffectAsset Replacement")]
public class VisualEffectReplacement : Hierarchy
{
    [field: SerializeField]
    public VisualEffectAsset VisualEffectAssetReplacement { get; private set; }

    public override void Apply(Transform rootTransform)
    {
        VisualEffect visualEffect = !string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).GetComponent<VisualEffect>() : rootTransform.GetComponent<VisualEffect>();
        visualEffect.visualEffectAsset = VisualEffectAssetReplacement;
    }
}