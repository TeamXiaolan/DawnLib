using Dawn;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New StoryLog Definition", menuName = $"{DuskModConstants.Definitions}/StoryLog Definition")]
public class DuskStoryLogDefinition : DuskContentDefinition<DawnStoryLogInfo>
{
    [field: SerializeField]
    public GameObject StoryLogGameObject { get; private set; }

    [field: SerializeField]
    public string Title { get; private set; }

    [field: SerializeField]
    public string Description { get; private set; }

    [field: SerializeField]
    public string Keyword { get; private set; }

    [field: Header("Optional")]
    [field: SerializeField]
    public TerminalNode? OverrideTerminalNode { get; private set; } = null;

    public StoryLogConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateStoryLogConfig(section);

        DawnLib.DefineStoryLog(TypedKey, StoryLogGameObject, builder =>
        {
            builder.OverrideTerminalNode(OverrideTerminalNode);
            builder.SetTitle(Title);
            builder.SetDescription(Description);
            builder.SetKeyword(Keyword);
            ApplyTagsTo(builder);
        });
    }

    public StoryLogConfig CreateStoryLogConfig(ConfigContext context)
    {
        return new StoryLogConfig
        {
        };
    }

    public override void TryNetworkRegisterAssets()
    {
        if (!StoryLogGameObject.TryGetComponent(out NetworkObject _))
            return;

        DawnLib.RegisterNetworkPrefab(StoryLogGameObject);
    }

    protected override string EntityNameReference => Title;
}