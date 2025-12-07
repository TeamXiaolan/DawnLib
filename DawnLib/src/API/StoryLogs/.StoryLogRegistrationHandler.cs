using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using MonoMod.RuntimeDetour;
using Unity.Netcode;
using UnityEngine;

namespace Dawn;

static class StoryLogRegistrationHandler
{
    internal static void Init()
    {
        using (new DetourContext(priority: int.MaxValue))
        {
            On.GameNetworkManager.Start += CollectAllStoryLogs;
        }
        On.Terminal.Start += RegisterStoryLogs;
    }

    private static void RegisterStoryLogs(On.Terminal.orig_Start orig, Terminal self)
    {
        orig(self);
        _ = TerminalRefs.Instance;
        List<CompatibleNoun> viewCompatibleNouns = TerminalRefs.ViewKeyword.compatibleNouns.ToList();
        List<TerminalKeyword> newTerminalKeywords = TerminalRefs.Instance.terminalNodes.allKeywords.ToList();
        foreach (DawnStoryLogInfo storyLogInfo in LethalContent.StoryLogs.Values)
        {
            if (storyLogInfo.ShouldSkipIgnoreOverride())
                continue;

            self.logEntryFiles.Add(storyLogInfo.StoryLogTerminalNode); // Needs to be done every lobby reload

            if (LethalContent.StoryLogs.IsFrozen)
                continue;

            storyLogInfo.StoryLogTerminalKeyword.defaultVerb = TerminalRefs.ViewKeyword;

            int index = self.logEntryFiles.Count - 1;
            storyLogInfo.StoryLogGameObject.GetComponent<StoryLog>().SetStoryLogID(index);
            storyLogInfo.StoryLogTerminalNode.storyLogFileID = index;

            CompatibleNoun compatibleNoun = new()
            {
                noun = storyLogInfo.StoryLogTerminalKeyword,
                result = storyLogInfo.StoryLogTerminalNode
            };

            viewCompatibleNouns.Add(compatibleNoun);
            newTerminalKeywords.Add(storyLogInfo.StoryLogTerminalKeyword);
        }

        if (LethalContent.StoryLogs.IsFrozen)
            return;

        TerminalRefs.Instance.terminalNodes.allKeywords = newTerminalKeywords.ToArray();
        TerminalRefs.ViewKeyword.compatibleNouns = viewCompatibleNouns.ToArray();

        foreach (CompatibleNoun compatibleNoun in TerminalRefs.ViewKeyword.compatibleNouns.Where(x => x.result.storyLogFileID > -1))
        {
            if (compatibleNoun.result == null || compatibleNoun.result.storyLogFileID < 0)
                continue;

            string name = NamespacedKey.NormalizeStringForNamespacedKey(compatibleNoun.result.creatureName, true);
            NamespacedKey<DawnStoryLogInfo>? key = StoryLogKeys.GetByReflection(name);
            if (key == null)
            {
                key = NamespacedKey<DawnStoryLogInfo>.From("unknown_lib", NamespacedKey.NormalizeStringForNamespacedKey(compatibleNoun.result.creatureName, false));
            }

            if (LethalContent.StoryLogs.ContainsKey(key))
            {
                DawnPlugin.Logger.LogWarning($"StoryLog {compatibleNoun.result.creatureName} is already registered by the same creator to LethalContent. This is likely to cause issues.");
                continue;
            }

            GameObject? storyLogGameObject = null;
            foreach (GameObject prefab in _networkPrefabStoryLogTypes)
            {
                if (prefab.GetComponent<StoryLog>().storyLogID == compatibleNoun.result.storyLogFileID)
                {
                    storyLogGameObject = prefab;
                    break;
                }
            }
            DawnStoryLogInfo storyLogInfo = new(key, [DawnLibTags.IsExternal], storyLogGameObject, compatibleNoun.result, compatibleNoun.noun, null);
        }
        LethalContent.StoryLogs.Freeze();
    }

    private static List<GameObject> _networkPrefabStoryLogTypes = new();
    private static void CollectAllStoryLogs(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
    {
        orig(self);
        foreach (NetworkPrefab networkPrefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
        {
            if (!networkPrefab.Prefab.TryGetComponent(out StoryLog _))
                continue;

            if (_networkPrefabStoryLogTypes.Contains(networkPrefab.Prefab))
            {
                continue;
            }

            _networkPrefabStoryLogTypes.Add(networkPrefab.Prefab);
        }
    }
}