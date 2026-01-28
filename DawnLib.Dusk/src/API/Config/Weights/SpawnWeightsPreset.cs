using System.Collections.Generic;
using Dawn;
using Dawn.Internal;
using Dusk.Weights.Transformers;
using UnityEngine;

namespace Dusk.Weights;

public class SpawnWeightsPreset : IWeighted, IContextualWeighted<SpawnWeightContext>
{
    public MoonWeightTransformer MoonSpawnWeightsTransformer { get; private set; } = null!;
    public InteriorWeightTransformer InteriorSpawnWeightsTransformer { get; private set; } = null!;
    public WeatherWeightTransformer WeatherSpawnWeightsTransformer { get; private set; } = null!;

    private readonly List<ISpawnWeightRule> _rules = new();
    private int _baseWeightIncrease;
    private bool _isSetup;

    public void SetupSpawnWeightsPreset(List<NamespacedConfigWeight> moonConfig, List<NamespacedConfigWeight> interiorConfig, List<NamespacedConfigWeight> weatherConfig, int baseWeightIncrease = 0)
    {
        MoonSpawnWeightsTransformer = new MoonWeightTransformer(moonConfig);
        InteriorSpawnWeightsTransformer = new InteriorWeightTransformer(interiorConfig);
        WeatherSpawnWeightsTransformer = new WeatherWeightTransformer(weatherConfig);
        _baseWeightIncrease = baseWeightIncrease;

        _rules.Clear();
        _rules.Add(new MoonRule(MoonSpawnWeightsTransformer));
        _rules.Add(new InteriorRule(InteriorSpawnWeightsTransformer));
        _rules.Add(new WeatherRule(WeatherSpawnWeightsTransformer));

        _isSetup = true;
    }

    public SpawnWeightsPreset AddRule(ISpawnWeightRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    public int GetWeight(SpawnWeightContext ctx)
    {
        if (!_isSetup)
        {
            return _baseWeightIncrease;
        }

        float weight = 0f;

        var applicable = new List<(int priority, int index, ISpawnWeightRule rule)>(_rules.Count);

        for (int i = 0; i < _rules.Count; i++)
        {
            var rule = _rules[i];
            if (!rule.CanApply(ctx))
                continue;

            int priority = Priority(rule.GetOperation(ctx));
            applicable.Add((priority, i, rule));
        }

        applicable.Sort(static (a, b) =>
        {
            int p = b.priority.CompareTo(a.priority);
            return p != 0 ? p : a.index.CompareTo(b.index);
        });

        foreach (var (priority, _, rule) in applicable)
        {
            Debuggers.Weights?.Log($"Old Weight: {weight}");
            weight = rule.Apply(weight, ctx);
            Debuggers.Weights?.Log($"New Weight: {weight}");
        }

        return Mathf.RoundToInt(weight + _baseWeightIncrease);
    }

    public int GetWeight()
    {
        DawnPlugin.Logger.LogFatal($"Using non-contextual GetWeight on SpawnWeightsPreset! This is likely a mistake.");
        SpawnWeightContext ctx = SpawnWeightContextFactory.FromCurrentGame();
        return GetWeight(ctx);
    }

    private static int Priority(MathOperation operation) => (operation == MathOperation.Additive || operation == MathOperation.Subtractive) ? 1 : 0;
}