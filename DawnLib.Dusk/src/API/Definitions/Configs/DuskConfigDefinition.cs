using BepInEx.Configuration;
using Dawn;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Config Definition", menuName = $"{DuskModConstants.Definitions}/Config Definition")]
public class DuskConfigDefinition : DuskContentDefinition, INamespaced<DuskConfigDefinition>
{
    [field: SerializeField]
    internal NamespacedKey<DuskConfigDefinition> _typedKey;

    [field: SerializeField]
    public DuskDynamicConfig DynamicConfig { get; internal set; }

    [field: SerializeField]
    public DuskContentDefinition ContentReference { get; internal set; }

    public NamespacedKey<DuskConfigDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<DuskConfigDefinition>(); }
    public object ConfigEntry { get; private set; }

    public override void Register(DuskRegistrationContext registrationContext)
    {
        base.Register(registrationContext);
        DuskBaseConfig? BaseConfig = (DuskBaseConfig?)ContentReference.BaseConfig;
        using ConfigContext context = registrationContext.Mod.ConfigManager.CreateConfigSectionForBundleData(registrationContext.AssetBundleData);
        ConfigEntryBase entry = registrationContext.Mod.ConfigManager.CreateDynamicConfig(BaseConfig?.UserAllowedToEdit() ?? true, DynamicConfig, context);
        registrationContext.Mod.configEntries.Add(entry);
        ConfigEntry = entry;
        DuskModContent.Configs.Register(this);
    }

    public override void TryNetworkRegisterAssets() { }

    protected override string EntityNameReference => DynamicConfig.settingName;
}