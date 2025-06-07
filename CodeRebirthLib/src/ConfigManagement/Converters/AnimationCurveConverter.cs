using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CodeRebirthLib.Exceptions;
using UnityEngine;

namespace CodeRebirthLib.ConfigManagement.Converters;
public class AnimationCurveConverter : TOMLConverter<AnimationCurve>
{
    protected override string ConvertToString(AnimationCurve value)
    {
        throw new System.NotImplementedException();
    }
    
    protected override AnimationCurve ConvertToObject(string keyValuePairs)
    {
        // Split the input string into individual key-value pairs
        string[] pairs = keyValuePairs.Split(';').Select(s => s.Trim()).ToArray();
        if (pairs.Length == 0)
        {
            if (int.TryParse(keyValuePairs, out int result))
            {
                return new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, result));
            }
            else
            {
                CodeRebirthLibPlugin.Logger.LogError($"Invalid key-value pairs format: {keyValuePairs}");
                return AnimationCurve.Linear(0, 0, 1, 0);
            }
        }
        List<Keyframe> keyframes = new();

        // Iterate over each pair and parse the key and value to create keyframes
        foreach (string pair in pairs)
        {
            string[] splitPair = pair.Split(',').Select(s => s.Trim()).ToArray();
            if (splitPair.Length == 2 &&
                float.TryParse(splitPair[0], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out float time) &&
                float.TryParse(splitPair[1], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                keyframes.Add(new Keyframe(time, value));
            }
            else
            {
                // this maybe shouldn't be an exception, but i don't really care.
                throw new MalformedAnimationCurveConfigException(pair);
            }
        }

        // Create the animation curve with the generated keyframes and apply smoothing
        var curve = new AnimationCurve(keyframes.ToArray());
        /*for (int i = 0; i < keyframes.Count; i++)
        {
            curve.SmoothTangents(i, 0.5f); // Adjust the smoothing as necessary
        }*/

        return curve;
    }
}