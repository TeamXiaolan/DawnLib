using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using LethalLevelLoader;
using UnityEngine;

namespace CodeRebirthLib.ModCompats;
static class LLLCompatibility
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(Plugin.ModGUID);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetCurveDictAndLevelTag(Dictionary<string, AnimationCurve> curvesByCustomLevel, SelectableLevel level, out string tagName)
    {
        tagName = string.Empty;
        ExtendedLevel? extendedLevel = PatchedContent.CustomExtendedLevels.FirstOrDefault(x => x.SelectableLevel == level) ?? PatchedContent.VanillaExtendedLevels.FirstOrDefault(x => x.SelectableLevel == level);
        if (extendedLevel == null) return false;
        foreach (var curve in curvesByCustomLevel)
        {
            foreach (ContentTag? tag in extendedLevel.ContentTags)
            {
                if (tag.contentTagName.ToLowerInvariant() == curve.Key)
                {
                    tagName = curve.Key;
                    return true;
                }
            }
        }
        return false;
    }
}