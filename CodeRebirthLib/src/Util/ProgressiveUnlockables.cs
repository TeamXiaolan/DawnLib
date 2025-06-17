using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeRebirthLib.Util;
public class ProgressiveUnlockables // TODO, rewrite basically
{
    public static Dictionary<UnlockableItem, bool> unlockableIDs = new();
    public static List<string> unlockableNames = new();
    public static List<TerminalNode> rejectionNodes = new();

    public static IEnumerator LoadUnlockedIDs(bool overrideUnlockables)
    {
        yield return new WaitUntil(() => CodeRebirthLibNetworker.Instance != null);
        for (int i = 0; i < unlockableIDs.Count; i++)
        {
            UnlockableItem unlockable = unlockableIDs.Keys.ElementAt(i);
            bool actuallyUnlocked = ES3.Load(unlockable.ToString(), false, CodeRebirthLibNetworker.Instance.SaveSettings);
            CodeRebirthLibPlugin.ExtendedLogging($"Unlockable {unlockable.unlockableName} is unlocked: {actuallyUnlocked}");
            unlockableIDs[unlockable] = actuallyUnlocked;
            unlockable.unlockableName = actuallyUnlocked ? unlockableNames[i] : "???";
        }
    }

    public static void SaveUnlockedIDs()
    {
        for (int i = 0; i < unlockableIDs.Count; i++)
        {
            UnlockableItem unlockable = unlockableIDs.Keys.ElementAt(i);
            bool unlocked = unlockableIDs[unlockable];
            ES3.Save(unlockable.ToString(), unlocked, CodeRebirthLibNetworker.Instance.SaveSettings);
            unlockableIDs[unlockable] = unlocked;
            int index = unlockableIDs.Keys.ToList().IndexOf(unlockable);
            unlockable.unlockableName = unlocked ? unlockableNames[index] : "???";
        }
    }
}