using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using CodeRebirthLib.Internal;
using CodeRebirthLib.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

public class CRMod
{
    private static readonly List<CRMod> _allMods = new();

    private readonly string _basePath;

    // todo: i dont like how many arguments are here lmao
    internal CRMod(Assembly assembly, BaseUnityPlugin plugin, AssetBundle mainBundle, string basePath, ConfigManager configManager) : this(MetadataHelper.GetMetadata(plugin.GetType()), mainBundle, basePath, configManager)
    {
        Assembly = assembly;
        ResolveCodeModInformation(assembly);
    }

    public static CRMod RegisterMod(BaseUnityPlugin plugin, AssetBundle mainBundle)
    {
        ConfigManager configManager = new(plugin.Config);
        return new CRMod(plugin.GetType().Assembly, plugin, mainBundle, Path.GetDirectoryName(plugin.GetType().Assembly.Location)!, configManager);
    }

    internal static CRMod RegisterNoCodeMod(CRModInformation modInfo, AssetBundle mainBundle, string basePath)
    {
        var plugin = modInfo.CreatePluginMetadata();
        Debuggers.CRMContentDefinition?.Log("Registering no-code mod!");
        ConfigManager configManager = new(ConfigManager.GenerateConfigFile(plugin));
        CRMod noCodeMod = new(plugin, mainBundle, basePath, configManager);
        noCodeMod.ModInformation = modInfo;
        noCodeMod.Logger = BepInEx.Logging.Logger.CreateLogSource(plugin.GUID);
        foreach (var assetBundleData in noCodeMod.Content.assetBundles)
        {
            _ = new DefaultContentHandler(noCodeMod);  // todo : i think i got pretty far in, there needs to be error handling for putting assetbundles next to eachother rather than mainbundle then Assets/subbundle
        }
        return noCodeMod;
    }

    private void ResolveCodeModInformation(Assembly assembly)
    {
        ModInformation = ScriptableObject.CreateInstance<CRModInformation>();
        var searchDir = Path.GetFullPath(assembly.Location);
        var parent = Directory.GetParent(searchDir);

        while (parent != null && !string.Equals(parent.Name, "plugins", StringComparison.OrdinalIgnoreCase))
        {
            searchDir = parent.FullName;
            parent = Directory.GetParent(searchDir);
        }

        if (searchDir.EndsWith(".dll"))
            return;

        var iconPath = Directory.EnumerateFiles(searchDir, "icon.png", SearchOption.AllDirectories).FirstOrDefault();
        ModInformation.ModIcon = LoadIcon(iconPath);

        var manifestPath = Directory.EnumerateFiles(searchDir, "manifest.json", SearchOption.AllDirectories).FirstOrDefault();
        string manifestContents = File.ReadAllText(manifestPath);
        ThunderstoreManifest manifest = JsonConvert.DeserializeObject<ThunderstoreManifest>(manifestContents)!;

        ModInformation.ModDescription = manifest.description;
        ModInformation.ModName = manifest.name;
        ModInformation.AuthorName = manifest.author_name;
        ModInformation.Version = manifest.version_number;
        ModInformation.ExtraDependencies = manifest.dependencies;
        ModInformation.WebsiteUrl = manifest.website_url;
        Debuggers.CRMContentDefinition?.Log($"Mod information found: {ModInformation.ModName}, {ModInformation.ModDescription}, {ModInformation.ModIcon != null}, {ModInformation.AuthorName}, {ModInformation.Version}, {ModInformation.ExtraDependencies}, {ModInformation.WebsiteUrl}");
    }

    private Sprite? LoadIcon(string iconPath)
    {
        if (iconPath == default)
            return null;

        var iconTex = new Texture2D(256, 256);
        if (!iconTex.LoadImage(File.ReadAllBytes(iconPath), true))
            return null;

        var ModIcon = Sprite.Create(iconTex, new Rect(0, 0, iconTex.width, iconTex.height), new Vector2(0.5f, 0.5f), 100);
        return ModIcon;
    }

    internal CRMod(BepInPlugin plugin, AssetBundle mainBundle, string basePath, ConfigManager configManager)
    {
        ConfigManager = configManager;
        _basePath = basePath;
        Plugin = plugin;

        ContentContainer[] containers = mainBundle.LoadAllAssets<ContentContainer>();
        if (containers.Length == 0)
        {
            throw new NoContentDefinitionInBundle(mainBundle);
        }
        if (containers.Length >= 2)
        {
            throw new MultipleContentDefinitionsInBundle(mainBundle);
        }

        Content = containers[0];
        _allMods.Add(this);
    }
    public static IReadOnlyList<CRMod> AllMods => _allMods.AsReadOnly();

    public ConfigManager ConfigManager { get; }
    public ContentContainer Content { get; }

    public Assembly? Assembly { get; }
    public CRModInformation ModInformation { get; set; }
    public ManualLogSource? Logger { get; set; }

    public BepInPlugin Plugin { get; }

    public string GetRelativePath(params string[] path)
    {
        return Path.Combine(_basePath, Path.Combine(path));
    }

    public bool TryGetRelativeFile(out string fullPath, params string[] path)
    {
        fullPath = GetRelativePath(path);
        return File.Exists(fullPath);
    }

    public bool TryGetBundleDataFromName(string bundleName, [NotNullWhen(true)] out AssetBundleData? data)
    {
        data = Content.assetBundles.FirstOrDefault(it => it.assetBundleName == bundleName);
        return data != null;
    }

    public void RegisterContentHandlers()
    {
        if (Assembly == null)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"Tried to Register Content Handlers for {Plugin.Name} but it is a no-code CRMod!");
            return;
        }

        IEnumerable<Type> contentHandlers = Assembly.GetLoadableTypes().Where(x =>
            !x.IsNested && x.BaseType != null
            && x.BaseType.IsGenericType
            && x.BaseType.GetGenericTypeDefinition() == typeof(ContentHandler<>)
        );

        foreach (Type type in contentHandlers)
        {
            type.GetConstructor([typeof(CRMod)]).Invoke([this]);
        }
    }
}