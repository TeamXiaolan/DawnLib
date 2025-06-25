# CodeRebirthLib

A Library to help manage large Lethal Company Mods. Makes registering with LethalLib/WeatherRegistry/etc easier and contains other useful scripts and utilities.

Currently supports:

## Lethal Lib

- Enemies
- Items
- Inside Map Objects
- Unlockables

## Native (CodeRebirthLib)

- Outside Map Objects

## PathfindingLib

- CodeRebirthLib contains a component that makes use of PathfindingLib to make it easy for entities to move in and out of interiors without any performance hit.

## Weather Registry (Soft Dependency)

- Weathers
You may want to include Weather Registry as a dependency in your thunderstore manifest.

### Setup - Both

You should have a Main AssetBundle ("main bundle" from here) that contains a `ContentContainer` ScriptableObject. This contains definitions to all the types of content that will be registered in your mod.

Each content bundle like `my_item_bundle` will contain an `AssetBundleData` ScriptableObject and as many content definitions as needed.

### Setup - Code/CSharp

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

There is a template for your plugin that you can use to get started:

- Start by making a folder somewhere on your pc to store the template, open something with an IDE like VSCode or VS and clone this github repository onto that folder <https://github.com/TeamXiaolan/CodeRebirthLib-Mod-Template>.
- And then navigate to a terminal and type `dotnet net install .`
- From there you can start using the template, do this by making a folder, preferrably something like your mod's name, for example, call the folder `CodeRebirth` (that's my mod's name, it's just an example).
- Then, if using Rider, and you know how to use C# templates, then um do it? I don't have Rider so I have no idea.
- But, if using VSCode or VS, type into the terminal the two commands:
- `dotnet new sln --name CodeRebirth`
- `dotnet new crlibmod -M com.github.xuuxiaolan.coderebirth -MM "PATH\TO\MMHOOK\FOLDER" -B "PATH\TO\MODMANAGER\PLUGINS\FOLDER" --name CodeRebirth`
- `-M` is the mod guid, `-MM` is the folder to mmhook files `-B` is folder to the bepinex plugin folder. also make sure to include `--name` otherwise it just dumps it wherever!?
- Finally, enter the project's folder you made with these commands and type in the final command: `dotnet tool install -g tcli` and from there you can do builds and stuff.

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
if (mod.WeatherRegistry().TryGetFromWeatherName("Meteor Shower", out CRWeatherDefinition? definition))
{
    // do something with the Meteor Shower definition.
    // i.e. grabbing the prefab.
    var prefab = definition.GameObject;
}
```

Finally just do a release build and everything should just be put into your mod manager's specific profile that you gave to the template.

### Setup - Editor

- If you have a Code/CSharp project setup for this then you only need to do a couple extra things.
- First, Create a new UnityProject: 2022.3.9f1 with 3D High Definition Rendering Pipeline (HDRP).
- Then add this package: <https://github.com/XuuXiaolan/CR-AssetBundle-Builder.git>.
- Then you need to patch your UnityProject with the latest lethal release, to do this go through the steps in nomnom's unity lc project patcher github repository: <https://github.com/nomnomab/unity-lc-project-patcher>.

CodeRebirthLib supports registering mods in the editor. However, it is untested. To register automatically you need to:

- make your main bundle have the extension `.crmod`
- contain a `ModInformation` Scriptable Object named exactly `Mod Information`

> Expected structure is:
>
> ```text
> yourmod.crmod
> my_item_bundle
> ```

### Setup - Extra Notes for the Editor portion

- The way you should be setting up your project in the editor is very simple, you need to go through this path after patching: `Assets/LethalCompany/Mods/plugins/` and create a folder there that's just your mod's name, so something like `CodeRebirth`.
- From there create another folder inside that and call it dependencies, this is where you'll go to thunderstore and manually download `CodeRebirthLib` and all its dependencies and just drag them in there so it should look something like this:

![Dependencies Image](https://i.postimg.cc/KjkGy2GH/image.png)

- Don't forget to go to the CodeRebirthLib dll and untick `Validate References` so it doesn't check for soft dependencies like LethalConfig or WeatherRegistry.
- Also if you ever need to update your unity project to the latest lethal version, you can just make a new unity project, patch that one with the latest lethal version, and drag your `ModNameFolder` into the plugins folder like how it is in the previous unity project you had, that's it.

> [!NOTE]
> If you use a vanilla asset in your unity project into your mod, make sure to move it somewhere in your own mod's folder, so that when you do update to latest lethal version, it doesnt get lost from accidently deleting it.

- Finally, if using CRAssetBundleBuilder (which is recommended), set your bundle output path to the `res` folder in your project if you have a CSharp/Code project, and if not, just make an `Assetbundles` folder next to `Dependencies` folder and put them all in there.

And finally, for any troubles in setting anything up, contact `@xuxiaolan` on discord for help.
