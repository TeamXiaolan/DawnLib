namespace Dawn;

public class AutoNonInteractableTagger : IAutoTagger<CRItemInfo>
{
    public NamespacedKey Tag => Tags.NonInteractable;
    public bool ShouldApply(CRItemInfo info)
    {
        return info.Item.spawnPrefab.TryGetComponent(out GrabbableObject grabbableObject) && grabbableObject.GetType() == typeof(GrabbableObject);
    }
}