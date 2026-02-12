using System.Collections.Generic;
using BepInEx.Configuration;
using Dawn;
using Dawn.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk;

public abstract class DuskContentDefinition : ScriptableObject
{
    public abstract NamespacedKey Key { get; protected set; }

    [FormerlySerializedAs("ConfigEntries")]
    [SerializeField]
    private List<DuskDynamicConfig> _configEntries = new();

    [SerializeField, UnlockedNamespacedKey]
    internal List<NamespacedKey> _tags = new();

    internal readonly Dictionary<string, ConfigEntryBase> generalConfigs = new();
    public DuskMod Mod { get; private set; }

    internal AssetBundleData AssetBundleData;

    public virtual void Register(DuskMod mod)
    {
        if (AssetBundleData == null)
        {
            mod.Logger?.LogError($"BUG! Tried to register {name} without setting AssetBundleData?");
            return;
        }

        Mod = mod;
        TryNetworkRegisterAssets();
    }

    public virtual void RegisterPost(DuskMod mod)
    {
        using ConfigContext context = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        foreach (DuskDynamicConfig configDefinition in _configEntries)
        {
            generalConfigs[configDefinition.settingName.CleanStringForConfig()] = mod.ConfigManager.CreateDynamicConfig(configDefinition, context);
        }
    }

    public abstract void TryNetworkRegisterAssets();

    public string GetDefaultKey()
    {
        string normalizedName = NamespacedKey.NormalizeStringForNamespacedKey(EntityNameReference, false);
        return normalizedName;
    }

    protected abstract string EntityNameReference { get; }
    protected void ApplyTagsTo(BaseInfoBuilder builder)
    {
        builder.SoloAddTags(_tags);
    }
}

public abstract class DuskContentDefinition<TInfo> : DuskContentDefinition where TInfo : INamespaced<TInfo>
{
    public NamespacedKey<TInfo> TypedKey => Key.AsTyped<TInfo>();

    [field: SerializeField, InspectorName("Namespace"), DefaultKeySource("GetDefaultKey", false)]
    public override NamespacedKey Key { get; protected set; }
}