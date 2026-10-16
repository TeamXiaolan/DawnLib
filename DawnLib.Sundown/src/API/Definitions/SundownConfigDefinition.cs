using BepInEx.Configuration;
using Dawn;
using Dusk;
using UnityEngine;

namespace Sundown;

[CreateAssetMenu(fileName = "New Config Definition", menuName = $"{SundownModConstants.Definitions}/Config Definition")]
public class SundownConfigDefinition : DuskContentDefinition, INamespaced<SundownConfigDefinition>
{
    [field: SerializeField]
    internal NamespacedKey<SundownConfigDefinition> _typedKey;

    [field: SerializeField]
    public SundownDynamicConfig DynamicConfig { get; internal set; }

    [field: SerializeField]
    public DuskContentDefinition ContentReference { get; internal set; }

    public NamespacedKey<SundownConfigDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<SundownConfigDefinition>(); }
    public object ConfigEntry { get; private set; }

    public void ManuallyRegister(DuskRegistrationContext registrationContext)
    {
        DuskBaseConfig? BaseConfig = ContentReference.BaseConfig;
        using ConfigContext context = registrationContext.Mod.ConfigManager.CreateConfigSectionForBundleData(registrationContext.AssetBundleData);
        ConfigEntryBase entry = ConfigManager.CreateDynamicConfig(BaseConfig?.UserAllowedToEdit() ?? true, DynamicConfig, context);
        registrationContext.Mod.configEntries.Add(entry);
        ConfigEntry = entry;
        SundownModContent.Configs.Register(this);
    }

    public override void TryNetworkRegisterAssets() { }

    protected override string EntityNameReference => DynamicConfig.settingName;
}