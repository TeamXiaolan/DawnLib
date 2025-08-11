using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BepInEx.Configuration;

namespace CodeRebirthLib;

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
internal static class Debuggers
{
    internal static DebugLogSource? ReplaceThis;

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