# CodeRebirthLib

A Library to help manage large Lethal Company Mods. Makes registering with Scrap/Enemies/Inside and Outside Hazards/Furniture/ShipUpgrades/WeatherRegistry/etc easier and contains other useful scripts and utilities.

Currently supports:

## Native (CodeRebirthLib)

- Enemies
- Scraps
- Shop Items
- Inside Map Objects
- Outside Map Objects
- Ship Upgrades
- Decors
- Skins (TODO)
- Achievements
- Tile Injection

## PathfindingLib

- CodeRebirthLib contains a component that makes use of PathfindingLib to make it easy for entities to move in and out of interiors without any performance hit.

## Weather Registry (Soft Dependency)

- Weathers
You may want to include Weather Registry as a dependency in your thunderstore manifest.

### Setup - Both

You should have a Main AssetBundle ("main bundle" from here) that contains a `ContentContainer` ScriptableObject. This contains definitions to all the types of content that will be registered in your mod.

Each content bundle like `my_item_bundle` will contain an `AssetBundleData` ScriptableObject and as many content definitions as needed.

### Setup - Code/CSharp

There is a template for your plugin that you can use to get started:

> Check the wiki section in thunderstore to see how to download the C# template and use it.

To register and get content out of the asset bundles in C# use:

> [!NOTE]
> Currently your packaged mod structure needs to be:
>
> ```text
> com.example.yourmod.dll
> assets/
> -> main_bundle
> -> my_item_bundle
> ```

```cs
// In your plugin
public static CRMod Mod { get; private set; }

private void Awake()
{
    AssetBundle mainBundle = CRLib.LoadBundle(Assembly.GetExecutingAssembly(), "main_bundle");
    Mod = CRLib.RegisterMod(this, mainBundle);
    Mod.Logger = Logger; // optional, but highly recommended

    // Load Content
    Mod.RegisterContentHandlers();
}
```

Then to divide up your content use the `ContentHandler` class to register. The specifics here might change slightly.

```cs
public class DuckContentHandler : ContentHandler<DuckContentHandler>
{
    public class DuckBundle : AssetBundleLoader<DuckBundle>
    {
        public DuckBundle(CRMod mod, string filePath) : base(mod, filePath)
        {

        }
    }

    public DuckContentHandler(CRMod mod) : base(mod)
    {
        RegisterContent("duckbundle", out DuckBundle assets); // returns bool on if it registered succesfully
    }
}
```

> For the above example and in cases where you don't need to retrive anything from the asset bundle `DefaultBundle` can be used instead (e.g. replacing the `DuckBundle` here)

After running `Mod.RegisterContentHandlers();` the registries in your `CRMod` will be populated. You can then get access to your content by running

```cs
if (mod.WeatherRegistry().TryGetFromWeatherName("Meteor Shower", out CRMWeatherDefinition? definition))
{
    // do something with the Meteor Shower definition.
    // i.e. grabbing the prefab.
    var prefab = definition.GameObject;
}
```

Finally just do a release build and everything should just be put into your mod manager's specific profile that you gave to the template.

### Setup - Unity Editor

- Check the UnityEditor section of the wiki on thunderstore to setup the Unity Editor.

And finally, for any troubles in setting anything up, contact `@xuxiaolan` on discord for help.

### Credits

- Bongo Loaforc (Code)
- XuXiaolan (Code)
- Slayer (Achievement UI)
