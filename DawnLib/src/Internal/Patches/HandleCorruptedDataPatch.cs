using System.Collections;
using TMPro;
using UnityEngine;

namespace Dawn.Internal;
static class HandleCorruptedDataPatch
{
    internal static void Init()
    {
        On.PreInitSceneScript.Awake += TryShowCorruptedWarning;
        On.PreInitSceneScript.EraseFileAndRestart += ErasePersistentDataContainers;
    }
    private static void ErasePersistentDataContainers(On.PreInitSceneScript.orig_EraseFileAndRestart orig, PreInitSceneScript self)
    {
        if (PersistentDataContainer.HasCorruptedData.Count > 0)
        {
            foreach (PersistentDataContainer container in PersistentDataContainer.HasCorruptedData)
            {
                container.DeleteFile();
            }
            self.StartCoroutine(DelayedClose(2));
            return;
        }
        orig(self);
    }
    static IEnumerator DelayedClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        Application.Quit();
    }
    private static void TryShowCorruptedWarning(On.PreInitSceneScript.orig_Awake orig, PreInitSceneScript self)
    {
        orig(self);
        int count = PersistentDataContainer.HasCorruptedData.Count;
        if (count > 0)
        {
            self.EnableFileCorruptedScreen();
            DawnPlugin.Logger.LogFatal($"The following {count} PersistentDataContainer(s) failed to load:");
            foreach (PersistentDataContainer container in PersistentDataContainer.HasCorruptedData)
            {
                DawnPlugin.Logger.LogFatal($" - {container.FileName}");
            }
            DawnPlugin.Logger.LogFatal("If you wish to try recover the saved information, please ALT+F4 now.");
            DawnPlugin.Logger.LogFatal("If you do not care, click the button to DELETE the missing files.");
            self.FileCorruptedDialoguePanel.transform.Find("NotificationText").GetComponent<TMP_Text>().text = $"[DawnLib] Failed to read {count} PersistentDataContainer(s), your files may be corrupted. Read the console for more information. If you do not care, click the button below to DELETE the missing files.";
        }
    }
}