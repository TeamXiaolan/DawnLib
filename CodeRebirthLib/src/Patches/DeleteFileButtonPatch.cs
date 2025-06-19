
using CodeRebirthLib.Util;

namespace CodeRebirthLib.Patches;
public static class DeleteFileButtonPatch
{
    public static void Init()
    {
        On.DeleteFileButton.DeleteFile += DeleteFileButton_DeleteFile;
    }

    private static void DeleteFileButton_DeleteFile(On.DeleteFileButton.orig_DeleteFile orig, DeleteFileButton self)
    {
        orig(self);
        ES3Settings settings;
        if (CodeRebirthLibNetworker.Instance != null)
        {
            settings = CodeRebirthLibNetworker.Instance.SaveSettings;
        }
        else
        {
            settings = new ES3Settings($"CRLibLCSaveFile{self.fileToDelete + 1}", ES3.EncryptionType.None);
        }
        CodeRebirthLibNetworker.ResetCodeRebirthLibData(settings);
    }
}