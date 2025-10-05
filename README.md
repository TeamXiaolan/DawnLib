# DawnLib

(was CodeRebirthLib)

DawnLib is a modern API for Lethal Company content and all sizes of mods. It contains:

- DawnLib API
- DuskMod API
- Some extra utilities

DawnLib also categorises (almost) everything in the game with keys, allowing for easy references to existing vanilla content.

**NOTE:** Enemies/Items/etc managed through DawnLib are likely **_unsupported_** by mods like CentralConfig or LethalQuantities.
This is because of the way DawnLib supports dynamically updating weights and therefore cannot be fixed from DawnLib.

## DawnLib (All C#)

```xml
<PackageReference Include="TeamXiaolan.DawnLib" Version="0.2.3" />

<!-- Optional Source Generation, mostly for when using the DuskMod API -->
<PackageReference Include="TeamXiaolan.DawnLib.SourceGen" Version="0.2.3" />
```

DawnLib is a hands-off way to register your content. The code to switch from LethalLib to DawnLib is very similar and will require minimal refactoring.
Most registration methods are under the `DawnLib` static class. When calling `DefineXXX` you are provided with a builder method that
explains most settings that you can configure.

Supported content:

- Enemies
- Items
- Map Objects
- Unlockables
- Additional Tile Sets

Example:

```csharp
public static class MeltdownKeys {
  public static readonly NamespacedKey<DawnItemInfo> GeigerCounter = NamespacedKey<DawnItemInfo>.From("facility_meltdown", "geiger_counter");
}

// In your plugin
DawnLib.DefineItem(MeltdownKeys.GeigerCounter, assets.geigerCounterItemDef, builder => builder
  .DefineShop(shopBuilder => shopBuilder
    .OverrideCost(90)
    .OverrideInfoNode(assets.geigerCounterNode)
  )
);
```

`LethalContent` is an easy way to reference vanilla/modded content:

```csharp
EnemyType blobEnemyType = LethalContent.Enemies[EnemyKeys.Blob].EnemyType;
```

It should be noted that the vanilla references will not be in the registry until a while after the lobby is created.
In order to make sure everything is ready, you can listen to a registry's "freeze" event.
`OnFreeze` will only run once _ever_ (even between lobby reloads)

```csharp
LethalContent.Enemies.OnFreeze += () => {
  // All vanilla content is in and no more modded content can be added.
};

if(LethalContent.Enemies.IsFrozen) { // or check that the registry has already been frozen
  // ...
}
```

### PersistentDataContainer

`PersistentDataContainer` is an alternative to `ES3`. You can easily access save data with the `.GetPersistentDataContainer()` extension method, `.GetCurrentContract()` or `.GetCurrentSave()`

```csharp
void Awake() { // Plugin awake
    PersistentDataContainer myContainer = this.GetPersistentDataContainer(); // use this however you want, note that 'this' is required to use the extension method in the Awake function.
    
    // these only return null when not in-game. these also automatically handle resetting the save
    PersistentDataContainer? contract = DawnLib.GetCurrentContract(); // resets on: getting fired and save deletion.
    PersistentDataContainer? save = DawnLib.GetCurrentSave(); // resets on: ONLY save deletion.
}
```

Note: If you are going to make a large edit (calling `.Set`, `.GetOrSet`, etc multiple times) you should wrap it with `using(container.LargeEdit())`. This delays saving data to the disk until all your edits have been completed.

## DuskMod (C# & Editor)

```xml
<PackageReference Include="TeamXiaolan.DawnLib" Version="0.2.3" />
<PackageReference Include="TeamXiaolan.DawnLib.DuskMod" Version="0.2.3" />

<!-- Optional Source Generation -->
<PackageReference Include="TeamXiaolan.DawnLib.SourceGen" Version="0.2.3" />
```

The DuskMod API is more opinionated, but automatically handles:

- Asset Bundle Loading
- Config Generation
- Skipping bundles when config is disabled
- Progressive Unlockables
- Automatically generate NamespacedKeys (C# Source Generators)
- **Registering content with no code!**

It also supports:

- Achievements
- Weathers (through `WeatherRegistry`)
- Vehicles (highly experimental)
- Enemy/Item Model Replacement

And finally, for any troubles in setting anything up, contact `@xuxiaolan` on discord for help.

### Credits

- Bongo Xiaolan (Code)
- XuXiaolan (Code)
- Slayer (Achievement UI)
