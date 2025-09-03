using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Dawn.Internal;
using Dawn.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Dawn.Dusk;

public class DuskMod
{
    private static readonly List<DuskMod> _allMods = new();

    private readonly string _basePath;

    internal DuskMod(Assembly assembly, BaseUnityPlugin plugin, AssetBundle mainBundle, string basePath, ConfigManager configManager) : this(MetadataHelper.GetMetadata(plugin.GetType()), mainBundle, basePath, configManager)
    {
        Assembly = assembly;
        ResolveCodeModInformation(assembly);
    }

    public static DuskMod RegisterMod(BaseUnityPlugin plugin, AssetBundle mainBundle)
    {
        ConfigManager configManager = new(plugin.Config);
        return new DuskMod(plugin.GetType().Assembly, plugin, mainBundle, Path.GetDirectoryName(plugin.GetType().Assembly.Location)!, configManager);
    }

    internal static DuskMod RegisterNoCodeMod(DuskModInformation modInfo, AssetBundle mainBundle, string basePath)
    {
        var plugin = modInfo.CreatePluginMetadata();
        Debuggers.Dusk?.Log("Registering no-code mod!");
        ConfigManager configManager = new(ConfigManager.GenerateConfigFile(plugin));
        DuskMod noCodeMod = new(plugin, mainBundle, basePath, configManager);
        noCodeMod.ModInformation = modInfo;
        noCodeMod.Logger = BepInEx.Logging.Logger.CreateLogSource(plugin.GUID);
        foreach (var assetBundleData in noCodeMod.Content.assetBundles)
        {
            _ = new DefaultContentHandler(noCodeMod);
        }
        return noCodeMod;
    }

    private void ResolveCodeModInformation(Assembly assembly)
    {
        ModInformation = ScriptableObject.CreateInstance<DuskModInformation>();
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
        Debuggers.Dusk?.Log($"Mod information found: {ModInformation.ModName}, {ModInformation.ModDescription}, {ModInformation.ModIcon != null}, {ModInformation.AuthorName}, {ModInformation.Version}, {ModInformation.ExtraDependencies}, {ModInformation.WebsiteUrl}");
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

    internal DuskMod(BepInPlugin plugin, AssetBundle mainBundle, string basePath, ConfigManager configManager)
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
    public static IReadOnlyList<DuskMod> AllMods => _allMods.AsReadOnly();

    public ConfigManager ConfigManager { get; }
    public ContentContainer Content { get; }

    public Assembly? Assembly { get; }
    public DuskModInformation ModInformation { get; set; }
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
            DawnPlugin.Logger.LogWarning($"Tried to Register Content Handlers for {Plugin.Name} but it is a no-code DuskMod!");
            return;
        }

        IEnumerable<Type> contentHandlers = Assembly.GetLoadableTypes().Where(x =>
            !x.IsNested && x.BaseType != null
            && x.BaseType.IsGenericType
            && x.BaseType.GetGenericTypeDefinition() == typeof(ContentHandler<>)
        );

        foreach (Type type in contentHandlers)
        {
            type.GetConstructor([typeof(DuskMod)]).Invoke([this]);
        }
    }
}