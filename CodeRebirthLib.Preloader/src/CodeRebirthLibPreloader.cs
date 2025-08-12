using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using Mono.Cecil;

namespace CodeRebirthLib.Preloader;
class CodeRebirthLibPreloader
{
    public const string GUID = MyPluginInfo.PLUGIN_GUID;
    public const string NAME = MyPluginInfo.PLUGIN_NAME;
    public const string VERSION = MyPluginInfo.PLUGIN_VERSION;
    
    internal static ManualLogSource Log { get; } = Logger.CreateLogSource("CodeRebirthLib.Preloader");
    
    public static IEnumerable<string> TargetDLLs { get; } = new string[] { "Assembly-CSharp.dll" };

    private static readonly string MainDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    
    public static void Patch(AssemblyDefinition assembly)
    {
        Log.LogWarning($"Patching {assembly.Name.Name}");
    }
    
    // Cannot be renamed, method name is important
    public static void Initialize()
    {
        Log.LogInfo($"Prepatcher Started");
    }

    // Cannot be renamed, method name is important
    public static void Finish()
    {
        Log.LogInfo($"Prepatcher Finished");
    }
}
