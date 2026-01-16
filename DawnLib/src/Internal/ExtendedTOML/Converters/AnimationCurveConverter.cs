using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Dawn.Internal;
class AnimationCurveConverter : TOMLConverter<AnimationCurve>
{
    protected override string ConvertToString(AnimationCurve value)
    {
        if (value == null || value.keys.Length == 0)
        {
            return ConvertToString(AnimationCurve.Constant(0, 1, 0));
        }

        const int minSamples = 10;

        Keyframe[] keys = value.keys;
        float startTime = keys[0].time;
        float endTime = keys[^1].time;

        int sampleCount = Math.Max(minSamples, keys.Length);

        string[] pairs = new string[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float delta = i / (sampleCount - 1f);
            float t = Mathf.Lerp(startTime, endTime, delta);
            float v = value.Evaluate(t);

            string timeStr = t.ToString("0.00", CultureInfo.InvariantCulture);
            string valueStr = v.ToString("0.00", CultureInfo.InvariantCulture);

            pairs[i] = $"{timeStr}, {valueStr}";
        }
        return string.Join(';', pairs);
    }

    protected override AnimationCurve ConvertToObject(string keyValuePairs)
    {
        string[] pairs = keyValuePairs.Split(';').Select(s => s.Trim()).ToArray();
        if (pairs.Length == 0)
        {
            if (int.TryParse(keyValuePairs, out int result))
            {
                return new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, result));
            }
            DawnPlugin.Logger.LogError($"Invalid key-value pairs format: {keyValuePairs}");
            return AnimationCurve.Constant(0, 1, 0);
        }
        List<Keyframe> keyframes = new();

        foreach (string pair in pairs)
        {
            string[] splitPair = pair.Split(',').Select(s => s.Trim()).ToArray();
            if (splitPair.Length == 2 &&
                float.TryParse(splitPair[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float time) &&
                float.TryParse(splitPair[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                keyframes.Add(new Keyframe(time, value));
            }
            else
            {
                // this maybe shouldn't be an exception, but i don't really care.
                MalformedAnimationCurveConfigException malformedAnimationCurveConfigException = new MalformedAnimationCurveConfigException(pair);
                malformedAnimationCurveConfigException.LogNicely(DawnPlugin.Logger);
                throw malformedAnimationCurveConfigException;
            }
        }

        AnimationCurve curve = new(keyframes.ToArray());
        /*for (int i = 0; i < keyframes.Count; i++)
        {
            curve.SmoothTangents(i, 0.5f); // Adjust the smoothing as necessary
        }*/

        return curve;
    }

    public override bool IsEnabled()
    {
        return !LethalQuantitiesCompat.Enabled;
    }
}