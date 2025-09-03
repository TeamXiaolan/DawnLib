using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BepInEx.Configuration;

namespace Dawn.Internal;

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
internal static class Debuggers
{
    internal static DebugLogSource? ReplaceThis;
    internal static DebugLogSource? Sounds;
    internal static DebugLogSource? Weathers;
    internal static DebugLogSource? Enemies;
    internal static DebugLogSource? Moons;
    internal static DebugLogSource? Items;
    internal static DebugLogSource? MapObjects;
    internal static DebugLogSource? Unlockables;
    internal static DebugLogSource? AssetLoading;
    internal static DebugLogSource? Weights;
    internal static DebugLogSource? Achievements;
    internal static DebugLogSource? CRMContentDefinition;
    internal static DebugLogSource? Progressive;
    internal static DebugLogSource? ExtendedTOML;
    internal static DebugLogSource? LethalConfig;
    internal static DebugLogSource? Pathfinding;
    internal static DebugLogSource? Tags;
    internal static DebugLogSource? Dungeons;

    internal static void Bind(ConfigFile file)
    {
        foreach (FieldInfo fieldInfo in typeof(Debuggers).GetFields(BindingFlags.Static | BindingFlags.NonPublic))
        {
            if (file.Bind("InternalDebugging", fieldInfo.Name, false, "Enable/Disable this DebugLogSource. Should only be true if you know what you are doing or have been asked to.").Value)
            {
                fieldInfo.SetValue(null, new DebugLogSource(fieldInfo.Name));
                CodeRebirthLibPlugin.Logger.LogDebug($"created a DebugLogSource for {fieldInfo.Name}!");
            }
            else
            {
                fieldInfo.SetValue(null, null);
                CodeRebirthLibPlugin.Logger.LogDebug($"no DebugLogSource for {fieldInfo.Name}.");
            }
        }
    }
}

internal class DebugLogSource(string title)
{
    internal void Log(object message)
    {
        CodeRebirthLibPlugin.Logger.LogDebug($"[Debug-{title}] {message}");
    }
}