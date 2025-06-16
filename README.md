## CodeRebirthLib
A Library to help manage large Lethal Company Mods. Makes registering with LethalLib/WeatherRegistry/etc easier and contains other useful scripts and utilities.

Currently supports:
#### Lethal Lib
- Enemies
- Items
- Inside Map Objects
- Unlockables

#### Native (CodeRebirthLib)
- Outside Map Objects

#### Weather Registry (Soft Dependency)
You may want to include Weather Registry as a dependency in your thunderstore manifest.
- Weathers

### Setup - Both
You should have a Main AssetBundle ("main bundle" from here) that contains a `ContentContainer` ScriptableObject. This contains definitions to all the types of content that will be registered in your mod.

Each content bundle like `my_item_bundle` will contain an `AssetBundleData` ScriptableObject and as many content definitions as needed.


### Setup - C#
To register and get content out of the asset bundles in C# use:

> [!NOTE]
> Currently your packaged mod structure needs to be:
> ```
> com.example.yourmod.dll
> assets/
> -> main_bundle
> -> my_item_bundle
> ```

```cs
// In your plugin
public static CRMod Mod { get; private set; }

void Awake() {
    AssetBundle mainBundle = CRLib.LoadBundle(Assembly.GetExecutingAssembly(), "main_bundle");
    Mod = CRLib.RegisterMod(this, mainBundle);
    Mod.Logger = Logger; // optional, but highly recommended

    // Load Content
    Mod.RegisterContentHandlers();
}
```

Then to divide up your content use the `ContentHandler` class to register. The specifics here might change slightly.
```cs
public class DuckContentHandler : ContentHandler<DuckContentHandler> {
	public class DuckBundle : AssetBundleLoader<DuckBundle> {
		public DuckBundle([NotNull] CRMod mod, [NotNull] string filePath) : base(mod, filePath) {
		}
	}
	
	public DuckContentHandler([NotNull] CRMod mod) : base(mod) {
		RegisterContent("ducksongassets2", out DuckBundle assets); // returns bool on if it registered succesfully
	}
}
```
> For the above example and in cases where you don't need to retrive anything from the asset bundle `DefaultBundle` can be used instead (e.g. replacing the `DuckBundle` here)

After running `Mod.RegisterContentHandlers();` the registries in your `CRMod` will be populated. You can then get access to your content by running
```cs
if(mod.WeatherRegistry().TryGetFromWeatherName("Meteor Shower", out CRWeatherDefinition? definition)) {
    // do something with the Meteor Shower definition.
}
```

### Setup - Editor
CodeRebirthLib supports registering mods in the editor. However, it is untested. To register automatically you need to:
- make your main bundle have the extension `.crmod`
- contain a `ModInformation` Scriptable Object named exactly `Mod Information`

> Expected structure is: 
> ```
> yourmod.crmod
> my_item_bundle
> ```