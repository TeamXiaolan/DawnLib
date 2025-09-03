# v0.1.0
Hello World!

## DawnLib
- Added registering content through `DawnLib`
- Added `WeightTable` and `CurveTable` so that weights can be updated dynamically in-game.
- Added `ITerminalPurchasePredicate` to allow some items to not be bought based on conditions.
- Removed `VanillaEnemies` and added `LethalContent`
- Added `tags`
- Allow registering new tilesets to dungeons

## DuskMod
**MIGRATING FROM CODEREBIRTHLIB:** You will need to remake your `Content Container`, `Mod Information` and content definitions. The general flow has not changed overall.
- Reworked `Content Container`:
  - `Entity Name` (from content definitions) and `Entity Name Reference` have been removed. Instead, content can just be referenced.
  - `Asset Bundle Name` is now a dropdown.
  - Button to source generated all `NamespacedKey`s to generate in C#.
- Added 4 types of `Achievements`: Discovery, Instant, Stat, Parent
- Reworked progressive data. Both shop items and unlockables can now be progressive through the new `Progressive Predicate` ScriptableObject.
- Content definitions can now apply `tags`
- Added `DuskModContent` for the `Achievement` registry.
- Moved `.RegisterMod()` from `CRLib` to `DuskMod`
- Added `DuskAdditionalTilesDefinition`

## Utilities / Misc
- Added `NetworkAudioSource`, `AssetBundleUtils`
- Removed `ExtendedLogging` and split it into `DebugLogSources` (yes this uses SoundAPI code.)