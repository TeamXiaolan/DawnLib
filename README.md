## CodeRebirthLib
A Library to help manage large Lethal Company Mods. Makes registering with LethalLib/WeatherRegistry/etc easier and contains other useful scripts and utilities.

### Setup
You should have a Main AssetBundle that contains a `ContentContainer` ScriptableObject. This contains definitions to all the types of content that will be registered in your mod.

> [!NOTE]
> Currently your packaged mod structure needs to be:
> ```
> com.example.yourmod.dll
> assets/
> -> main_bundle
> -> my_item_bundle
> ```

Each content bundle like `my_item_bundle` will contain an `AssetBundleData` ScriptableObject and as many content definitions as needed.

```cs
// In your plugin
public static CRMod Mod { get; private set; }

void Awake() {
    AssetBundle mainBundle = CodeRebirthLib.LoadBundle(Assembly.GetExecutingAssembly(), "main_bundle");
    Mod = CodeRebirthLib.RegisterMod(this, mainBundle);
    Mod.Logger = Logger; // optional

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
		if(TryLoadContentBundle("ducksongassets2", out DuckBundle assets)) {
			LoadAllContent(assets!);
		}
	}
}
```

After running `Mod.RegisterContentHandlers();` the registries in your `CRMod` will be populated. You can then get access to your content by running
```cs
if(mod.Weathers.TryGetFromWeatherName("Meteor Shower", out CRWeatherDefinition? definition)) {
    // do something with the Meteor Shower definition.
}
```