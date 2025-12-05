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
        On.Terminal.Awake += RegisterStoryLogs;
    }

    private static void RegisterStoryLogs(On.Terminal.orig_Awake orig, Terminal self)
    {
        orig(self);
        _ = TerminalRefs.Instance;
        List<CompatibleNoun> viewCompatibleNouns = TerminalRefs.ViewKeyword.compatibleNouns.ToList();
        foreach (DawnStoryLogInfo storyLogInfo in LethalContent.StoryLogs.Values)
        {
            if (storyLogInfo.ShouldSkipIgnoreOverride())
                continue;

            self.logEntryFiles.Add(storyLogInfo.StoryLogTerminalNode); // Needs to be done every lobby reload

            if (LethalContent.StoryLogs.IsFrozen)
                continue;

            storyLogInfo.StoryLogTerminalKeyword.defaultVerb = TerminalRefs.ViewKeyword;

            int index = self.logEntryFiles.Count;
            storyLogInfo.StoryLogGameObject.GetComponent<StoryLog>().SetStoryLogID(index);
            storyLogInfo.StoryLogTerminalNode.storyLogFileID = index;

            CompatibleNoun compatibleNoun = new()
            {
                noun = storyLogInfo.StoryLogTerminalKeyword,
                result = storyLogInfo.StoryLogTerminalNode
            };

            viewCompatibleNouns.Add(compatibleNoun);
        }

        if (LethalContent.StoryLogs.IsFrozen)
            return;

        TerminalRefs.ViewKeyword.compatibleNouns = viewCompatibleNouns.ToArray();

        foreach (GameObject storyLogGameObject in _networkPrefabStoryLogTypes)
        {
            if (storyLogGameObject.GetComponent<DawnStoryLogNamespacedKeyContainer>())
            {
                Debuggers.StoryLogs?.Log($"Already registered {storyLogGameObject}");
                continue;
            }

            CompatibleNoun compatibleNoun = TerminalRefs.ViewKeyword.compatibleNouns.Where(x => x.result.storyLogFileID == storyLogGameObject.GetComponent<StoryLog>().storyLogID).FirstOrDefault();

            string name = NamespacedKey.NormalizeStringForNamespacedKey(compatibleNoun.result.creatureName, true);
            NamespacedKey<DawnStoryLogInfo>? key = StoryLogKeys.GetByReflection(name);
            if (key == null)
            {
                key = NamespacedKey<DawnStoryLogInfo>.From("unknown_lib", NamespacedKey.NormalizeStringForNamespacedKey(compatibleNoun.result.creatureName, false));
            }

            if (LethalContent.StoryLogs.ContainsKey(key))
            {
                DawnPlugin.Logger.LogWarning($"StoryLog {compatibleNoun.result.creatureName} is already registered by the same creator to LethalContent. This is likely to cause issues.");
                DawnStoryLogNamespacedKeyContainer duplicateContainer = storyLogGameObject.AddComponent<DawnStoryLogNamespacedKeyContainer>();
                duplicateContainer.Value = key;
                continue;
            }

            TerminalNode terminalNode = compatibleNoun.result;
            TerminalKeyword terminalKeyword = compatibleNoun.noun;
            DawnStoryLogInfo storyLogInfo = new(key, [DawnLibTags.IsExternal], storyLogGameObject, terminalNode, terminalKeyword, null);
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