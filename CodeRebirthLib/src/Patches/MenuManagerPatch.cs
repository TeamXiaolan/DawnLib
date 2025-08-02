using System.Collections.Generic;
using CodeRebirthLib.ContentManagement.Achievements;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.Util;
using UnityEngine;
using UnityEngine.Audio;

namespace CodeRebirthLib.Patches;

static class MenuManagerPatch
{

    private static HashSet<GameObject> _prefabsToFix = new();
    private static bool _alreadyFixedAllPrefabs = false;
    internal static void Init()
    {
        On.MenuManager.Start += MenuManager_Start; // TODO: save achievements if they're done in the main menu too.
    }

    private static void MenuManager_Start(On.MenuManager.orig_Start orig, MenuManager self)
    {
        orig(self);
        VanillaLevels.Init();
        CRAchievementHandler.LoadAll();
        DoAchievementUI(self);
        DoSoundFixes(self);
    }

    private static void DoSoundFixes(MenuManager menuManager)
    {
        AudioSource? menuManagerAudioSource = menuManager.GetComponent<AudioSource>();
        if (menuManagerAudioSource == null)
        {
            // can this even happen
            return;
        }

        AudioMixer audioMixer = menuManagerAudioSource.outputAudioMixerGroup.audioMixer; // store this and reuse it in FixMixerGroups for fixing later audiosources?
        foreach (GameObject prefabToFix in _prefabsToFix)
        {
            AudioSource[] audioSourcesToFix = prefabToFix.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource audioSource in audioSourcesToFix)
            {
                if (audioSource.outputAudioMixerGroup == null || audioSource.outputAudioMixerGroup.audioMixer.name != "NonDiagetic") // huh why does LL ignore it if it's null or not NonDiagetic?
                    continue;

                AudioMixerGroup? audioMixerGroup = audioMixer.FindMatchingGroups(audioSource.outputAudioMixerGroup.name)[0];
                if (audioMixerGroup == null)
                    continue;

                audioSource.outputAudioMixerGroup = audioMixerGroup;
                CodeRebirthLibPlugin.ExtendedLogging("Set mixer group for " + audioSource.name + " in " + prefabToFix.name + " to NonDiagetic:" + audioMixerGroup.name);
            }
        }
        _alreadyFixedAllPrefabs = true;
        _prefabsToFix.Clear();
    }

    public static void FixCRLibMixerGroups(GameObject prefab)
    {
        if (_alreadyFixedAllPrefabs)
        {
            // fix them, assumes it's after menumanager so im sure we can save a reference to the appropriate audioMixerGroup needed, is this even needed?
            return;
        }

        if (_prefabsToFix.Contains(prefab))
            return;

        _prefabsToFix.Add(prefab);
    }

    private static void DoAchievementUI(MenuManager menuManager)
    {
        var canvas = GameObject.Instantiate(CodeRebirthLibPlugin.Main.AchievementUICanvasPrefab, menuManager.transform.parent.Find("MenuContainer"));
        canvas.GetComponent<AchievementUICanvas>()._menuManager = menuManager;

        if (AchievementUIGetCanvas.Instance == null)
            Object.Instantiate(CodeRebirthLibPlugin.Main.AchievementGetUICanvasPrefab);

        var menuContainer = GameObject.Find("MenuContainer");
        if (!menuContainer)
            return;

        var mainButtonsTransform = menuContainer.transform.Find("MainButtons");
        if (!mainButtonsTransform)
            return;

        var quitButton = mainButtonsTransform.Find("QuitButton");
        if (!quitButton)
            return;

        MenuUtils.InjectMenu(mainButtonsTransform, quitButton.gameObject);
    }
}