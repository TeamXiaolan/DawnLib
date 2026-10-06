using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;

namespace Daybreak;

public class DaybreakPlugin
{
    internal static Harmony? Harmony { get; set; }
    internal static ManualLogSource Log { get; } = Logger.CreateLogSource(nameof(DaybreakPlugin));

    public static IEnumerable<string> TargetDLLs { get; } = [];

    public static Action OnModsLoaded = delegate { };

    public static void Patch(AssemblyDefinition assembly) { }

    // Cannot be renamed, method name is important
    public static void Initialize() { }

    // Cannot be renamed, method name is important
    public static void Finish()
    {
        Harmony = new Harmony("DawnLib.Daybreak");
        Harmony.PatchAll(typeof(DaybreakPlugin).Assembly);
    }
}