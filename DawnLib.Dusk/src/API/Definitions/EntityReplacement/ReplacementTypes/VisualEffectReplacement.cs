using UnityEngine;
using UnityEngine.VFX;

namespace Dusk;

[CreateAssetMenu(fileName = "New VisualEffectAsset Replacement", menuName = $"Entity Replacements/Replacements/VisualEffectAsset Replacement")]
public class VisualEffectReplacement : HierarchyReplacement
{
    [field: SerializeField]
    public VisualEffectAsset VisualEffectAssetReplacement { get; private set; }

    public override void Apply(Transform rootTransform)
    {
        VisualEffect visualEffect = rootTransform.Find(HierarchyPath).GetComponent<VisualEffect>();
        visualEffect.visualEffectAsset = VisualEffectAssetReplacement;
    }
}