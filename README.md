# DawnLib

[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/TeamXiaolan/DawnLib/.github%2Fworkflows%2Fbuild.yml?logo=github)](https://github.com/TeamXiaolan/DawnLib/blob/main/.github/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/TeamXiaolan.DawnLib?logo=nuget)](https://www.nuget.org/packages?q=TeamXiaolan.DawnLib)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/TeamXiaolan/DawnLib?logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/TeamXiaolan/DawnLib?logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/)

(was CodeRebirthLib)

DawnLib is a modern API for Lethal Company content and all sizes of mods. It contains:

- DawnLib API
  - Moons
  - Interiors
  - Weathers
  - Terminal Commands
  - FootstepSurfaces (With gravity control and ability to create custom vain shroud skins)
  - Enemies
  - Items
  - Map Objects (Inside and Outside hazards)
  - Unlockables (Ship Upgrades and Furniture)
  - Additional Tile Sets (injecting more tilesets to interiors)
  - Dead Bodies
  - Round Loading Steps

- DuskMod API
  - Ship Creation (In-progress)
  - Achievements
  - Entity Replacements (Enemy, Item, Unlockable and MapObject skin replacements)
  - Vehicles (Highly experimental)
  - StoryLogs

- Some extra utilities
  - SmartAgentNavigator (completely intelligent NavMeshAgent navigator that uses PathfindingLib to performanently be able to use entrance teleports and fire exits reliably).
  - NetworkAudioSource
  - And more!

DawnLib also categorises (almost) everything in the game with keys, allowing for easy references to existing vanilla content.
If you'd like to support in the development of DawnLib, please consider reaching out to `@xuxiaolan` on discord, whether through the modding discord, github, or just
pinging on the lethal modding discord, we accept any sort of help, UI, Coders, Artists, etc.

**NOTE:** Enemies/Items/etc managed through DawnLib are likely **_unsupported_** by mods like CentralConfig or LethalQuantities.
This is because of the way DawnLib supports dynamically updating weights and therefore cannot be fixed from DawnLib.
For this, DawnLib has collaborated with the creator `@Crafty` that made the mod `LunarConfig` which does super similar functionality and gives the user a lot of control!

## DawnLib (All C#)

```xml
<PackageReference Include="TeamXiaolan.DawnLib" Version="1.*" />

<!-- Optional Source Generation, mostly for when using the DuskMod API -->
<PackageReference Include="TeamXiaolan.DawnLib.SourceGen" Version="1.*" />
```

DawnLib is a hands-off way to register your content. The code to switch from LethalLib to DawnLib is very similar and will require minimal refactoring.
Most registration methods are under the `DawnLib` static class. When calling `DefineXXX` you are provided with a builder method that
explains most settings that you can configure.

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
`OnFreezeWithContext` will only run once _ever_ (even between lobby reloads)

```csharp
LethalContent.Enemies.OnFreezeWithContext += (NamespacedKeyResolver resolver) =>
{
  // All vanilla content is in and no more modded content can be added.
};

if (LethalContent.Enemies.IsFrozen)
{ // or check that the registry has already been frozen
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

Note: If you are going to make a large edit (calling `.Set`, `.GetOrSet`, etc multiple times) you should wrap it with `using(container.CreateEditContext())`. This delays saving data to the disk until all your edits have been completed.

### Weight Sources & Modifiers

`WeightProfile` is how DawnLib handles giving things like items, enemies, etc weights on moons, interiors, etc.
This is done by having a priority-based list of `WeightModifier`'s, which modify the base weight (usually 0 or an empty animation curve for map objects) one by one until getting a final result.
`WeightSource`'s are used to create `WeightModifier`'s, this is because modifiers need to contain hard references via `NamespacedKey`'s into moons, interiors, etc, Otherwise there would be no reliable way of validating whether a modifier can apply.
Most `WeightModifier`'s are built automatically by iterating through all sources on the `WeightProfile` on the Freeze callbacks of the various registries, items, enemies, etc.
`WeightSource`'s are added to `WeightProfile`'s via the `.AddSource` method.
`WeightSource`'s and `WeightModifier`'s work like pairs and are able to be created by mods other than DawnLib to apply to sources for your items etc.
Finally, `WeightContextContributor` take the current context given on what's asking for the weights and what moons it was given and stores something using that data.
For example, here is an `ItemInfoWeightContextContributor` that takes the `Owner` of the weight context call and tries to validate whether it's an item calling said call.

```csharp
using Dawn;

namespace Dusk.Weights;

public sealed class ItemInfoWeightContextContributor : IWeightContextContributor
{
    public void Contribute(WeightContextBuilder builder)
    {
        object? owner = builder.Query.Owner;
        if (owner == null || owner is not DawnItemInfo itemInfo)
            return;

        builder.Set(DuskWeightContextKeys.ItemInfo, itemInfo);
    }
}
```

The `WeightContextContributor` also has to be registered to DawnLib to be used, preferrably in your Plugin's `Awake` method, like so:
```csharp
DawnLib.Weights.AddContextContributor(new ItemInfoWeightContextContributor());
```

The builder can be used later inside of the `WeightModifier` to grab the DawnItemInfo found above.

Below is an example of a `WeightSource` and `WeightModifier` pair used to give equivalent weight of an item skin based on the number of skins the item already has.
```csharp
using System.Collections.Generic;
using Dawn;

namespace Dusk.Weights;

public sealed class DynamicItemSkinSource : WeightModifierSource<int>
{
    private readonly NamespacedKey _keyOfRelevantSkin;

    public DynamicItemSkinSource(NamespacedKey keyOfRelevantSkin)
    {
        _keyOfRelevantSkin = keyOfRelevantSkin;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        // Creating the modifier and giving it the key of the skin that we're calculating the weight for.
        // Sometimes, modifiers can be config-based for the user, who might not know the NamespacedKey for things like moons.
        // And for that there are other ways to use an "UnresolvedNamespacedKey" and turn it into a normal NamespacedKey afterwards on the Build call via NamespacedKeyResolver's.
        modifiers.Add(new DynamicItemSkinModifier(_keyOfRelevantSkin));
    }
}
```

```csharp
using System.Collections.Generic;
using Dawn;

namespace Dusk.Weights;

public sealed class DynamicItemSkinModifier : IWeightModifier<int>
{
    private readonly NamespacedKey _keyOfRelevantSkin;

    public DynamicItemSkinModifier(NamespacedKey keyOfRelevantSkin)
    {
        _keyOfRelevantSkin = keyOfRelevantSkin;
    }

    public NamespacedKey Key => DuskKeys.DynamicItemSkin;

    public WeightModifierPhase Phase => WeightModifierPhase.Base;

    public int Priority => 0;

    public bool CanApply(WeightContext context)
    {
        if (!context.TryGet(DuskWeightContextKeys.ItemInfo, out DawnItemInfo? itemInfo))
            return false;

        if (itemInfo != context.Owner)
            return false;

        return true;
    }

    public void Apply(ref int value, WeightContext context)
    {
        if (!context.TryGet(DuskWeightContextKeys.ItemInfo, out DawnItemInfo? itemInfo))
        {
            // ItemInfo not found, shouldn't happen though.
            return;
        }

        if (!itemInfo.CustomData.TryGet(DuskKeys.EntityReplacements, out List<DuskEntityReplacementDefinition>? replacements))
        {
            // No skins found, shouldn't happen though.
            return;
        }

        int totalWeightOfAllReplacementsExceptRelevant = 0;
        foreach (DuskEntityReplacementDefinition replacementDefinition in replacements)
        {
            if (replacementDefinition.Key == _keyOfRelevantSkin)
            {
                // Skin trying to grab weight from the skin we applied this weightmodifier to.
                continue;
            }

            totalWeightOfAllReplacementsExceptRelevant += replacementDefinition.GetRarity(context.Moon, context.Dungeon, context.Weather, false);
        }

        // Equalise weight of this item's skin to be the same as the other item skins combined.
        value = totalWeightOfAllReplacementsExceptRelevant;
    }
}
```

## DuskMod (C# & Editor)

```xml
<PackageReference Include="TeamXiaolan.DawnLib" Version="1.*" />
<PackageReference Include="TeamXiaolan.DawnLib.DuskMod" Version="1.*" />

<!-- Optional Source Generation -->
<PackageReference Include="TeamXiaolan.DawnLib.SourceGen" Version="1.*" />
```

The DuskMod API is more opinionated, but automatically handles:

- Asset Bundle Loading
- Config Generation
- Skipping bundles when config is disabled
- Progressive Unlockables
- Automatically generate NamespacedKeys (C# Source Generators)
- **Registering content with no code!**

And finally, for any troubles in setting anything up, contact `@xuxiaolan` on discord or github for help.

### Credits - Maintainers/Main Contributors

- [Bongo Xiaolan](https://github.com/LoafOrc)
- [Xu Xiaolan](https://github.com/XuuXiaolan)
- [Pacoito](https://github.com/pacoito123)
- [Darmuh](https://github.com/darmuh)
- [Fumo](https://github.com/xntkrnl)
- [Ratijas](https://github.com/ratijas)

### Credits - Misc

- Crafty (Making LunarConfig exposed a few places that needed improvement in implementation).
- [Slayer](https://github.com/slayer6409) (Achievement UI)
- Monty (Hotloading UI Feedback)
- IAmBatby (UI+General Feedback)
- Zaggy (Advice + other stuff I forgor)
- Matty (Preloader stuff)
- Scoops (Unity Editor support + their goated base game shader work)

### Credits - Testers

- Crafty
- Kiszony
- TheCheeseXD
- Boom Hen
- Zerowe
- DistinctBlaze
- SkittyMuffins
