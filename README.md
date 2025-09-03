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
<PackageReference Include="TeamXiaolan.DawnLib" Version="0.10.0" />

<!-- Optional Source Generation, mostly for when using the DuskMod API -->
<PackageReference Include="TeamXiaolan.DawnLib.SourceGen" Version="0.10.0" />
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
`OnFreeze` will only run once *ever* (even between lobby reloads)
```csharp
LethalContent.Enemies.OnFreeze += () => {
  // All vanilla content is in and no more modded content can be added.
};

if(LethalContent.Enemies.IsFrozen) { // or check that the registry has already been frozen
  // ...
}
```

## DuskMod (C# & Editor)
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

And finally, for any troubles in setting anything up, contact `@xuxiaolan` on discord for help.

### Credits

- Bongo Loaforc (Code)
- XuXiaolan (Code)
- Slayer (Achievement UI)
