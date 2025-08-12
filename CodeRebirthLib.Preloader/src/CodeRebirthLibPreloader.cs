using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Mono.Cecil;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace CodeRebirthLib.Preloader;
class CodeRebirthLibPreloader
{
    internal static ManualLogSource Log { get; } = Logger.CreateLogSource("CodeRebirthLib.Preloader");
    
    public static IEnumerable<string> TargetDLLs { get; } = new string[] { "Assembly-CSharp.dll" };
    private static string[] HasExtraCRInfo = ["SelectableLevel", "EnemyType", "Item", "UnlockableItem", "DunGen.TileSet", "DunGen.Graph.DunGenFlow"];
    
    public static void Patch(AssemblyDefinition assembly)
    {
        Log.LogWarning($"Patching {assembly.Name.Name}");

        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            if (HasExtraCRInfo.Contains(type.FullName))
            {
                //TypeReference reference = new TypeReference("CodeRebirthLib", "CRMoonInfo", module, module.TypeSystem.CoreLibrary);
                type.AddField(FieldAttributes.Public, "__crinfo", assembly.MainModule.TypeSystem.Object);
            }
        }
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
