using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Enemies;
[SuppressMessage("ReSharper", "IdentifierTypo", Justification = "All these variables are named to match the scriptable object name")]
public static class VanillaLevels
{
    private static readonly List<SelectableLevel> _allLevels = new();
    public static IReadOnlyList<SelectableLevel> AllLevels => _allLevels.AsReadOnly();

    public static SelectableLevel CompanyBuildingLevel { get; private set; }
    public static SelectableLevel ExperimentationLevel { get; private set; }
    public static SelectableLevel MarchLevel { get; private set; }
    public static SelectableLevel VowLevel { get; private set; }
    public static SelectableLevel AssuranceLevel { get; private set; }
    public static SelectableLevel OffenseLevel { get; private set; }
    public static SelectableLevel RendLevel { get; private set; }
    public static SelectableLevel DineLevel { get; private set; }
    public static SelectableLevel TitanLevel { get; private set; }
    public static SelectableLevel AdamanceLevel { get; private set; }
    public static SelectableLevel ArtificeLevel { get; private set; }
    public static SelectableLevel EmbrionLevel { get; private set; }
    public static SelectableLevel LiquidationLevel { get; private set; }

    public static bool IsVanillaLevel(SelectableLevel level) // is this stupid?
    {
        return level == CompanyBuildingLevel || level == ExperimentationLevel || level == MarchLevel || level == VowLevel || level == AssuranceLevel || level == OffenseLevel || level == RendLevel || level == DineLevel || level == TitanLevel || level == AdamanceLevel || level == ArtificeLevel || level == EmbrionLevel || level == LiquidationLevel;
    }

    internal static void Init()
    {
        List<string> unknownTypes = [];

        var levels = Resources.FindObjectsOfTypeAll<SelectableLevel>();
        for (int i = 0; i < levels.Length; i++)
        {
            _allLevels.Add(levels[i]);
            CodeRebirthLibPlugin.ExtendedLogging($"Found level: {levels[i].name}");

            PropertyInfo property = typeof(VanillaLevels).GetProperty(levels[i].name);
            if (property == null) unknownTypes.Add(levels[i].name);
            else property.SetValue(null, levels[i]);
            // instead of this findobjectsoftypeall? or just wait for startofround.instance to be ready
        }

        CodeRebirthLibPlugin.ExtendedLogging($"Unknown levels: {string.Join(", ", unknownTypes)}");
    }
}