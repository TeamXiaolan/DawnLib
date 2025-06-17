using CodeRebirthLib.Extensions;
using CodeRebirthLib.Util;

namespace CodeRebirthLib.ContentManagement.Unlockables.Progressive;
public class ProgressiveUnlockData
{
    public CRUnlockableDefinition Definition { get; }
    public bool IsUnlocked { get; private set; }

    public const string LOCKED_NAME = "???";
    public string OriginalName { get; private set; }
    
    private UnlockableItem _unlockable => Definition.UnlockableItemDef.unlockable;
    
    private string _saveID => _unlockable.ToString(); // todo: something better than this
    
    public ProgressiveUnlockData(CRUnlockableDefinition definition)
    {
        Definition = definition;
        OriginalName = _unlockable.unlockableName;
        
        ProgressiveUnlockableHandler.AllProgressiveUnlockables.Add(this);
    }

    public void Unlock(HUDDisplayTip? displayTip = null)
    {
        if(IsUnlocked) return;
        IsUnlocked = true;

        _unlockable.unlockableName = OriginalName;

        if (displayTip != null)
        {
            HUDManager.Instance.DisplayTip(displayTip);
        }
    }
    
    public void Load(ES3Settings settings)
    {
        IsUnlocked = ES3.Load(_saveID, false, settings);
        // todo: add override?
        _unlockable.unlockableName = IsUnlocked ? OriginalName : LOCKED_NAME;
    }

    public void Save(ES3Settings settings)
    {
        ES3.Save(_saveID, IsUnlocked, settings);
    }
}