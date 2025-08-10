using System;
using System.Globalization;
using System.Linq;
using BepInEx.Logging;

namespace CodeRebirthLib;
public class MalformedAnimationCurveConfigException(string pair) : Exception
{
    public string Pair { get; } = pair;

    public void LogNicely(ManualLogSource? logger)
    {
        if (logger == null) return;

        string[] splitPair = Pair.Split(',').Select(s => s.Trim()).ToArray();
        logger.LogError($"Invalid key,value pair format: '{Pair}'! More details:");
        logger.LogError($"   Found '{splitPair.Length}' parts");

        if (splitPair.Length != 2)
        {
            logger.LogError("   FAIL -> Should have only 2 parts!");
            return;
        }

        LogNicelyFloatParse(logger, "first", splitPair[0]);
        LogNicelyFloatParse(logger, "second", splitPair[1]);
    }

    private void LogNicelyFloatParse(ManualLogSource logger, string context, string value)
    {
        logger.LogError($"   Attempting to parse {context} value: '{value}'");
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
        {
            logger.LogError($"   Parsed {context} value! Parsed Value is {parsedValue}");
        }
        else
        {
            logger.LogError($"   FAIL -> Couldn't parse {context} value! Parsed Value is {parsedValue}");
        }
    }
}