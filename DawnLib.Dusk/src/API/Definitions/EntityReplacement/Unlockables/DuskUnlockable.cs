using Dawn.Preloader.Interfaces;
using UnityEngine;

namespace Dusk;

public class DuskUnlockable : MonoBehaviour, ICurrentEntityReplacement
{
    public object? CurrentEntityReplacement { get; set; }

    public DuskUnlockableReplacementDefinition? GetUnlockableReplacement()
    {
        DuskUnlockableReplacementDefinition? unlockableReplacementDefinition = (DuskUnlockableReplacementDefinition?)CurrentEntityReplacement;
        return unlockableReplacementDefinition;
    }

    internal bool HasUnlockableReplacement()
    {
        return GetUnlockableReplacement() != null;
    }

    internal void SetUnlockableReplacement(DuskUnlockableReplacementDefinition unlockableReplacementDefinition)
    {
        CurrentEntityReplacement = unlockableReplacementDefinition;
    }

    public void Awake() {   }

    public void Start() { }

    public void OnDestroy() { }
}