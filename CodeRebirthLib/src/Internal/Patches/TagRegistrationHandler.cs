using System;

namespace CodeRebirthLib.Internal;

// todo: it might be nice to have a global event listener for common things like StartOfRound.Awake? idk its probably not needed though
static class TagRegistrationHandler
{
    internal static event Action OnApplyTags = delegate { };
    private static bool _hasAppliedTags;

    internal static void Init()
    {
        On.StartOfRound.Start += (orig, self) =>
        {
            orig(self);
            if(_hasAppliedTags) return;
            
            OnApplyTags.Invoke();
            _hasAppliedTags = true;
        };
    }
}