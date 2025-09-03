namespace Dawn.Internal;

static class ExtraItemEventsPatch
{
    internal static void Init()
    {
        On.GrabbableObject.OnBroughtToShip += GrabbableObject_OnBroughtToShip;
    }

    private static void GrabbableObject_OnBroughtToShip(On.GrabbableObject.orig_OnBroughtToShip orig, GrabbableObject self)
    {
        orig(self);
        if (ExtraItemEvents.eventListeners.TryGetValue(self, out ExtraItemEvents events))
        {
            events.onCollectInShip.Invoke();
        }
    }
}