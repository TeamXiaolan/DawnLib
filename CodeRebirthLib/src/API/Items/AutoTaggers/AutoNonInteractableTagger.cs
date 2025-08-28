namespace CodeRebirthLib;

public class AutoNonInteractableTagger(NamespacedKey tag) : IAutoTagger<CRItemInfo>
{
    public NamespacedKey Tag => tag;
    public bool ShouldApply(CRItemInfo info)
    {
        return info.Item.spawnPrefab.TryGetComponent(out GrabbableObject grabbableObject) && grabbableObject.GetType() == typeof(GrabbableObject);
    }
}