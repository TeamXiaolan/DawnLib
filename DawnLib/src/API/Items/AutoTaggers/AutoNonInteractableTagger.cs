namespace Dawn;

public class AutoNonInteractableTagger : IAutoTagger<DawnItemInfo>
{
    public NamespacedKey Tag => Tags.NonInteractable;
    public bool ShouldApply(DawnItemInfo info)
    {
        return info.Item.spawnPrefab.TryGetComponent(out GrabbableObject grabbableObject) && grabbableObject.GetType() == typeof(GrabbableObject);
    }
}