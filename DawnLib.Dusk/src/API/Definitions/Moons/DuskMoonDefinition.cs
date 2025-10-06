using System;
using System.Collections.Generic;
using Dawn;
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
                    mod.GetRelativePath("Assets", sceneData.BundleName),
                    sceneData.ScenePath
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
    public NamespacedKey<MoonSceneInfo> Key;
    
    [AssetBundleReference]
    public string BundleName;

    public string ScenePath;
}