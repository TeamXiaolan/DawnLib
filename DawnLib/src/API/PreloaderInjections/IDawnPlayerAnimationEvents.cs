using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(PlayerAnimationEvents))]
public interface IDawnPlayerAnimationEvents
{
    public void PlayCrouchFootstepSound();
}