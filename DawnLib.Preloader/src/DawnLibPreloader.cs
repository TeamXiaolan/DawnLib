using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Mono.Cecil;

namespace Dawn.Preloader;
class DawnLibPreloader
{
    internal static ManualLogSource Log { get; } = Logger.CreateLogSource("DawnLib.Preloader");
    private static readonly Dictionary<string, Dictionary<string, List<TypeDefinition>>> Interfaces = [];
    public static IEnumerable<string> TargetDLLs { get; } = new string[] { "Assembly-CSharp.dll" };

    public static void Patch(AssemblyDefinition assembly)
    {
        static void logHandler(bool fail, string message)
        {
            if (fail)
                Log.LogWarning(message);
            else
                Log.LogDebug(message);
        }

        if (Interfaces.TryGetValue(assembly.Name.Name, out var dict))
        {
            Log.LogWarning($"Patching {assembly.Name.Name}");
            foreach (var type in assembly.MainModule.Types)
            {
                if (!dict.TryGetValue(type.Name, out var list))
                    continue;

                foreach (var @interface in list)
                {
                    if (!type.ImplementInterface(@interface, logHandler))
                        break;
                }
            }
        }

        /*
        var outputAssembly = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{assembly.Name.Name}.pdll";
        Log.LogWarning($"Saving modified Assembly to {outputAssembly}");
        assembly.Write(outputAssembly);*/
    }

    // Cannot be renamed, method name is important
    public static void Initialize()
    {
        Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME}Prepatcher Started");
        // PluginConfig.Init();

        var pluginPath = Directory.EnumerateDirectories(Paths.PluginPath).FirstOrDefault(d => d.Contains("DawnLib"));

        if (pluginPath == null)
        {
            Log.LogFatal("Could not find plugins path! DawnLib not found at path: " + Paths.PluginPath);
            return;
        }

        var dllPath = Path.Combine(pluginPath, "DawnLib", "com.github.teamxiaolan.dawnlib.interfaces.dll");

        if (!File.Exists(dllPath))
        {
            Log.LogFatal("Could not find Interfaces dll!");
            return;
        }

        var interfaceAssembly = AssemblyDefinition.ReadAssembly(dllPath);

        var attributeName = typeof(InjectInterfaceAttribute).FullName;

        foreach (var type in interfaceAssembly.MainModule.Types)
        {
            if (!type.IsInterface)
                continue;

            var attributes = type.CustomAttributes.Where(at => at.AttributeType.FullName == attributeName);

            foreach (var customAttribute in attributes)
            {
                var attr = customAttribute.GetAttributeInstance<InjectInterfaceAttribute>();

                if (!Interfaces.TryGetValue(attr.AssemblyName, out var dict))
                {
                    Interfaces[attr.AssemblyName] = dict = [];
                }

                if (!dict.TryGetValue(attr.TypeName, out var list))
                {
                    dict[attr.TypeName] = list = [];
                }

                list.Add(type);
            }
        }
    }

    // Cannot be renamed, method name is important
    public static void Finish()
    {
        Log.LogInfo($"Prepatcher Finished");
    }
}
