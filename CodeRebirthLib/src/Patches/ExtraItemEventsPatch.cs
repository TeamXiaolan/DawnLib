using CodeRebirthLib.ContentManagement.Items;

namespace CodeRebirthLib.Patches;
static class ExtraItemEventsPatch
{
    internal static void Init()
    {
        On.GrabbableObject.OnBroughtToShip += (orig, self) =>
        {
            orig(self);
            if (ExtraItemEvents.eventListeners.TryGetValue(self, out ExtraItemEvents events))
            {
                events.onCollectInShip.Invoke();
            }
        };
    }
}