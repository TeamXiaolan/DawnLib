using Dawn.Utils;

namespace Dawn.Internal;
static class DebugPatches
{
    internal static void Init()
    {
        On.QuickMenuManager.Start += AddDebugFixes;
        On.TMPro.TMP_Dropdown.Show += TMPDropdownShow;
        On.TMPro.TMP_Dropdown.Hide += TMPDropdownHide;
    }

    private static void TMPDropdownHide(On.TMPro.TMP_Dropdown.orig_Hide orig, TMPro.TMP_Dropdown self)
    {
        if (self.gameObject.TryGetComponent(out TMPDropDownFixer dropDownFixer))
        {
            dropDownFixer.OnCloseDropdown();
        }
        orig(self);
    }

    private static void TMPDropdownShow(On.TMPro.TMP_Dropdown.orig_Show orig, TMPro.TMP_Dropdown self)
    {
        orig(self);
        if (self.gameObject.TryGetComponent(out TMPDropDownFixer dropDownFixer))
        {
            dropDownFixer.OnOpenDropdown(self);
        }
    }

    private static void AddDebugFixes(On.QuickMenuManager.orig_Start orig, QuickMenuManager self)
    {
        orig(self);
        self.allItemsDropdown.gameObject.AddComponent<TMPDropDownFixer>();
        self.debugEnemyDropdown.gameObject.AddComponent<TMPDropDownFixer>();
    }
}