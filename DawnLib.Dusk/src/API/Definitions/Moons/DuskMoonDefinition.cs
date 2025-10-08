using System;
using System.Collections.Generic;
using Dawn;
using Dusk.Utils;
using Dusk.Weights;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Moon Definition", menuName = $"{DuskModConstants.Definitions}/Moon Definition")]
public class DuskMoonDefinition : DuskContentDefinition<DawnMoonInfo>
{
    [field: SerializeField]
    public SelectableLevel Level { get; private set; }

    [SerializeField]
    private List<DuskMoonSceneData> _scenes = [];

    [field: SerializeField]
    public DuskTerminalPredicate TerminalPredicate { get; private set; }

    [field: SerializeField]
    public DuskPricingStrategy PricingStrategy { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);

        DawnLib.DefineMoon(TypedKey, Level, builder =>
        {
            foreach (DuskMoonSceneData sceneData in _scenes)
            {
                builder.AddScene(
                    sceneData.Key,
                    sceneData.Weight(),
                    mod.GetRelativePath("Assets", sceneData.BundleName),
                    sceneData.Scene.ScenePath
                );
            }

            if (TerminalPredicate)
            {
                builder.SetPurchasePredicate(TerminalPredicate);
            }

            if (PricingStrategy)
            {
                builder.OverrideCost(PricingStrategy);
            }
        });
    }

    protected override string EntityNameReference => Level?.PlanetName ?? string.Empty;
}

[Serializable]
public class DuskMoonSceneData
{
    public SceneReference Scene;
    public string BundleName => Scene.BundleName;

    [InspectorName("Namespace")]
    public NamespacedKey<IMoonSceneInfo> Key;

    [field: SerializeField]
    public int BaseWeight { get; private set; } = 100;
    [field: SerializeField]
    public string WeatherWeights { get; private set; } = "None=*1, DustClouds=*1, Rainy=*1, Stormy=*1, Foggy=*1, Flooded=*1, Eclipsed=*1";

    public int Weight() // todo: this shouldn't return an int but an IProviderTable<int>??
    {
        SpawnWeightsPreset newSpawnWeightsPreset = new();
        newSpawnWeightsPreset.SetupSpawnWeightsPreset(string.Empty, string.Empty, WeatherWeights);
        return BaseWeight + newSpawnWeightsPreset.GetWeight();
    }
}