# v0.5.13

- Backported a fix for ClientNetworkTransform's that used to work but didn't after the `Netcode for GameObjects` update.
- Added the `WeightedOutcomeScript`, which allows you to roll a select/random amounts of weighted events.

## v0.5.12

- Fixed DuskUnlockable not going to every UnlockableItem as it should be (the non-modded ones).

## v0.5.11

- Fixed DawnLib items and unlockables not making the purchased sound on buying.
- Fixed soft dependency issue caused by linq.

## v0.5.10

- Dungeon blank references uses network prefabs list.
- Added more null checks.
- Started supporting Vector3 and Vector2 for network variables.

## v0.5.9

- Added compatibility with LLL clamping.
- Added interior clamping.
- Added a null check in the map objects.
- Fixed a small issue with smart matching not always working.
- Fixed an issue where a config with no additive or substractive weights would work

## v0.5.8

- Fixed vanilla story logs not being registered because zeekerss never actually made prefabs for them.
- Added a config to give LLL control over vanilla moon visibility whether they're locked or hidden, defaulted to false.
- Made it cleaner to add storylogs' text.

## v0.5.7

- Fixed another minor issue with DatePredicate in some edge cases.

## v0.5.6

- Added a fallback for unity being weird about assets not being picked up sometimes.

## v0.5.5

- Fixed a null ref with some story log mods that add story logs late.

## v0.5.4

- Added Storylog registration.
- Added `DawnStoryLogSpawner` component to utils addcomponent menu.
- Properly implemented smart matching.
- Fixed an obsolete editor oopsie with interior and moon weights for entity replacements.
- Fixed interior mapobjects blanks not being replaced.
- Created `StoryLogKeys` for vanilla story logs.
- Moved where most registries freeze to be after a harmony patch of startofround.start prefix for more compat with more mods that add items etc late.
- Publicised a few internal methods.
- Updated wiki to adding a new page on how to make the editor look more like base-game thanks to help by `Scoops`.
- Fixed a bunch of issues relating to entity replacements not working properly in some edge cases.

## v0.5.3

- Not sure how I missed this issue when testing: fixed not being able to play?
- Created a migrator for the new saving.

## v0.5.2

- Reverted save system to what it was before temporarily to create a migration system.

## v0.5.1

- Optimised saving system a lot more, it should be a lot faster.
- If you have a current ongoing save, disable the saving system so it does not delete every item and unlockable in that save, then re-enable it when you make a new save.
- Made editor experience cleaner again

## v0.5.0

- Optimised the save system even more.
- Interior hotloading is real.
- Added more options for outside object conditions like ground tag, ship and entrance positions, AI node count, etc.
- Added NamespacedKey smart matching, so you should be able to do stuff like `oxyde` without typing in `code_rebirth:oxyde`, needs more testing but should work.

## v0.4.12

- Made weight configs more user friendly to avoid unexpected issues.

## v0.4.11

- Polished vehicle stuff on the station side a bit more.
- Hotfix for shop items breaking on lobby reload.

## v0.4.10

- Fixed an issue where the DatePredicate would not run for the entity replacements.
- Improved the Editor UI for spawn weight related options in the different definitions, i.e. enemies, items, etc.
- Updated Editor dll for better readability in some cases.
- Improved vehicle registration, it should technically work now?
- Internally added a `TryRegisterItemToShop` method for non DawnLib items
- Internally edited some fields to be private rather than read-only.

## v0.4.9

- Cleaned up credits a bit more.
- Fixed an issue where depending on the pc's locale (country I think?) some configs would get generated incorrectly.
- Added more logging for the preloader issue that some people rarely have.

## v0.4.8

- Added more null checks just incase.
- Actually added the GiantKiwi fix what.
- Fixed an issue with some interior stuff being null.

## v0.4.7

- Fixed issue with GiantKiwi just lying to me about what it has in-game.

## v0.4.6

- Fixed issues relating to joining multiplayer and landing on a moon.
Not entirely sure how those happened but they did ig.

## v0.4.5

- Edited MapObjects in the editor to be much easier to add onto.
- Added Interior priority onto MapObject SpawnWeights.
- Fixed issue with interior registration weights.
- Added blank reference replacing with interior SpawnSyncedObjects, butlerbees, enemy ai nests, haunted mask, sapsucker, lungprop script.
- Fixed another issue with unlockables when saving.
- Gave DawnLib Interiors SpawnWeight configs.
- Gave DawnLib Moons a lot more configs.
- Did more work onto documentation and made the editor experience cleaner.

## v0.4.4

- Fixed a small bug with an oversight in storing unlockables.
- Fixed some editor side tinks and made it a bit cleaner to create content related to moons.
- Updated the documentation in the DawnLib's thunderstore Wiki page.

## v0.4.3

- Fixed incompat with moresuits.
- Gave more configs to moons.
- Fixed a long standing issue with registering no-code mods when you have multiple pieces of content.

## v0.4.2

- Added support for `PlayerControllerReference`, `int` and `double` in `NetworkVariable`s
- Added components from DawnLib and DuskMod to be visible within the list in the Add Component Menu
- Added `PlanetUnlocker` a useable grabbable object to unlock a planet that uses `ProgressivePredicate`
- Actually implemented the OutOfBounds fix from MattyFixes, I accidently forgot.
- Removed some unneeded obsolete fields like AlwaysKeepLoaded

## v0.4.1

- Removed some redundant code.
- Implemented an OutOfBounds fix from MattyFixes into here due to having a saving system.
- Fixed client being able to pull lever during the start of the game that would break the lever.
- Readme and wiki updated with more information about how to create content with DawnLib.

## v0.4.0

- Fixed incompat with LCBetterSave.
- Totally didn't screw up the content order thing as well.

## v0.3.16

- Accidently had ContentOrder as internal, changed to public.

## v0.3.15

- Fixed another issue created by some unlockables that caused errors.
- Fixed an incompat between LLL, DawnLib and DarmuhsTerminalStuff that made every moon hidden.
- Added a config to disable achievements button in the main menu.

## v0.3.14

- Made unlockables parent to the ship to remove a stuttering issue when landing or leaving.
- Fixed an issue with replacements where the rotation sometimes isn't accurate depending on parent.

## v0.3.13

- Fixed a lobby reload softlock issue.
- Fixed issue where only half of the unlockables would save.
- Removed facility's `#natural` tag.

## v0.3.12

- Added `ContentOrder` attribute to apply to content handlers if you'd like them to run in specific order.
- Fixed client/host desync in unlockable skins and unlockable id causing position desyncs.
- Touched up UI a little bit.

## v0.3.11

- Potentially fixed issues regarding save file not being deleted properly.
- Added ways to edit the item's holding rotation, resting rotation, position offsets etc for skins.
- Fixed incompat with mod UnlockOnStart.
- Fixed incompat with mod TooManySuits.
- Touched up hotloading UI a bit. (will do more later)

## v0.3.10

- Fixed an issue where all replacements wouldn't take more than 1 replacement at once.
- Added MapObject replacements.
- Fixed DatePredicate being inaccurate.
- Made buying unlockables clear terminal text.

## v0.3.9

- Added more logging for a specific issue that shouldn't really happen?
- Added a `WaitAction` to replacements so you can have a wait happen inbetween actions.
- Fixed an issue where the combination of the mods WeatherTweaks, TerminalFormatter and OpenMonitors would break the ship monitor.
- Cleaned up the hotloading UI a bit more.
- Created ProgressiveMoonScrap that when interacted with, unlocks a specified moon and destroys itself.
- Added more config options to unlockable definition

## v0.3.8

- Fixed a compat issue with TwoRadarMap.
- Fixed registering issues with MoreSuits.
- Fixes regarding soft dependencies breaking.
- Fixes regarding UI to achievements tab.
- Fixed issues regarding LL shop items not being registered potentially
- Fixed issues regarding items not spawning on top of furniture as they should be, including fixing some vanilla furniture that would have forced items to spawn under, like the following:
  - Table
  - Romantic Table
  - Signal Transmitter
  - Sofa
  - Microwave
  - Fridge
  - Electric Chair
  - Dog House

## v0.3.7

- Nothing cuz i accidently pushed it.

## v0.3.6

- Got rid of DawnGrabbableObject, replaced with a preloader injected interface, DawnSaveData
- Fixed incompat with LL items not getting registered

## v0.3.5

- Replaced vanilla save system (atleast for items rn only) with DawnLib's own system.
  - Also allows for the rotations of items to be retained in comparison to vanilla resetting rotations.
  - Also allows you to get rid of and add mods that add items without your save's items getting scrambled.
  - Added config to opt out of it in case of issues or dislike I guess.
- Added UnlockableReplacementDefinition for replacing the model etc of unlockables
- Fixed an issue with Unlockables sometimes registering as suits depending on setup errors.
- Added a loading bar that tracks each player's progress for moons with DawnLib hotloading!

## v0.3.4

- Fixed a small issue with custom moon terminal loading.
- Added proper soft compat with LLL hiding and locking moons.

## v0.3.3

- Fixed SkinnedMeshReplacement removing all base materials.
- Allowed materials to be replaced with null to hide.
- Edited 1 character in the SmartAgentNavigator class that should fix a lot of pathing issues, this was so stupid.
- Added `GameObjectEditor` for doing stuff like moving, rotating, deleting and/or disabling gameobjects.
- Fixed an issue with Artifice and Embrion where because they're hidden you would get rejected from landing on them by default.

## v0.3.2

- Added MaterialPropertiesReplacement to EntityReplacements.
- Added ScanNodeReplacement to EntityReplacements.
- Gave devs support to NetworkVariable<bool> and NetworkVariable<float>.
- Added tag: `#dawn_lib:has_buying_percent` to make your moon have the buying percentage similar to company.
- Fixed some lag issues in the Editor.dll.
- Fixed replacing blank SO's of all the ones that LLL replaced (and more) in DawnLib content.
- Changed MapObjects to use NamespacedKeys.
- Inside MapObjects spawn more performantly now too.
- Fixed a small issue with hotloading where loading too early into a moon unlocks the lever too early.

## v0.3.1

- Switched from using quaternion on entity replacements, I realised that if even I don't get how they work, I can't expect anyone else to also understand that.

## v0.3.0

- **Update to v73. This version and versions after are not compatible with v72 or lower! (revert to v0.2.16 for v72)**
- Added support for `Moons` and `Dungeons`. Please note that this is very experimental at the moment (compatibility for LLL is also unknown)
  - Moons have multi-scene support.
  - Moons also have custom landing animation support.

- Changes to entity replacement, idk what.
- Added date for a `DuskPredicate`
- Added ItemKeys for the new scrap introduced in v73.
- New `#lethal_company:body_parts` tag.
- New `TerminalPredicateCollection` and `PredicateCollection` to allow using multiple predicates on shop items.
- `DuskAdditionalTilesDefinition` can now use a `DuskPredicate` to determine when the tiles should be injected

## v0.2.16

- Fixed issue with parent achievements not disappearing if they aren't possible.
- Fixed issue where inside hazards would not spawn.
- Fixed issue with being unable to move furniture properly with furniture lock mod.
- Fixed issue where SID started including items that they shouldn't because those items had 0 weights on the moon.

## v0.2.15

- Fixed issues with some specific scrap mods that caused a lot of scraps and other parts of the mod to break.
- Added some more error safety with using incorrect config formats.
- Made some weather fields internal rather than public.

## v0.2.13

- Added more debugging for ship unlockables not being registered properly via other mods.

## v0.2.12

- Fixed weathers being unregistered on lobby reload.

## v0.2.11

- Added a date filter predicate to the entity replacement definitions.
- Serialized some fields from UnlockProgressiveObject that should've been serialized.

## v0.2.10

- Changed some bestiary node behaviour to do with enemies.
- Fixed issues with Ship Upgrades and Decors not being assigned the correct ID's.
- Edited the spacing in the ContentDefinitions for enemies items and entity replacements to give the user more space to write configs.
- Fixed DawnLib Scrap not saving after a game restart.

## v0.2.9

- Fixed issues with ship upgrades and decors not respawning or not working with furniturelock mod.

## v0.2.8

- Fixed more achievement issues for when they're disabled.

## v0.2.7

- Fixed an issue with multiple mods adding achievements

## v0.2.6

- Added more config error checking to the Definitions as this mod uses a different format compared to other mods.
- Added the tag `custom` to the mod as `lethal_company:custom` or just `custom` should work.
- Entity, Enemy and Item content definitions have defaults weights of 0 on every vanilla moon, multipliers of 1 on every vanilla weather and default weight of 0 on every vanilla interior.
- Fixed `SpawnWeightsPreset` entirely not working causing nothing to have any weights.
- Fixed Weight Updater depending on weather never running thus nothing causing everything to not have any weights.

## v0.2.5

- Edited Editor.dll to be depending on the latest DawnLib version.
- Fixed spawn weight issues with items, enemies and entity replacements where the config gets replaced with an empty config.

## v0.2.4

- Fixed an issue relating to buying custom vehicles.
- Fixed some issues with some moons not setting up their dungeon generator correctly.
- Fixed some issues with some enemy creators COUGH ALICE COUGH not setting up the enemy on the right timing.

## v0.2.3

- Fixed issues with editor configs not being generated by the content definitions.
- Exposed some normal game refs
- Updated readme a bit.
- Added LunarConfig tag to DawnLib incase Crafty wants to add that tag to everything and then change the DawnInfo so it makes things easier to edit.
- Added `InDropShipAnimation` to VehicleAPI for vehicle currently in drop ship animation.

## v0.2.2

- Fixed some issues with the Entity Replacements bricking the game, again bwaa.
- Butler bees should now be accessible in the enemy registry.

## v0.2.1

- Fixed some issues with the Entity Replacements bricking the game, yw.

## v0.2.0

- Implemented near-complete-prototype of EntityReplacementDefinition
  - Contains EnemyReplacementDefinition and ItemReplacementDefinition currently, but potentially more to come.
  - Usable with custom enemies and items too, but requires the editor for the script to be auto generated, check the wiki or ask in the discord thread for DawnLib for more info.
- Improved Editor tools a bit more in terms of NamespacedKey formatting and other areas like the package building picking up assetbundles.
- Removed any configs or reference to `DuskContentDefinition`s from `ContentContainer`, moved all configs to the `DuskContentDefinition` itself and added a button to just migrate old configs to new places.
- Fix parent achievements not registering other achievements as complete properly.
- Got rid of WR dependency on the editor, so now you just need DawnLib, the DawnLib editor tool and PathfindingLib.
- Fixed issues with UnlockableItems (i.e. ship upgrades) not showing up in the terminal.
- Fixed .AddWeight not working properly.
- Removed validation from the Editor.dll temporarily to move it into the `DuskContentDefinition`s themselves later.
- Did a bit more work on vehicle registration with terminal pricing strategies and terminal predicates.

## v0.1.9

- Fixed an issue with some enemies like from pikmin not registering.

## v0.1.8

- Added `On First Scan` event to `ExtraScanEvents`.
- Fixed issues with other mods adding duplicate items into the list of all items.
- Fixed more compatibilities with other mods.
- Made sure to fix issues with achievement popup not working properly.

## v0.1.7

- Attempt at fixing some more file locking issues or whatever.
- Fixed a bunch of the definitions being broken in editor.

## v0.1.6

- Improved error handling with corrupted save data and tagging.
- Fixed an issue with applying item spawn group tags.
- Added `ExtraScanEvents` with an `On Scan` event (requires `Scan Node Properties`)
- Refined VehicleBase + VehicleStation code to be compatible with dropship attaching etc.

## v0.1.5

- Fixed issues with LLL shop items not being proper keyword names by LLL.
- Fixed achievement UI sound not playing and breaking the achievements.
- Fixed an issue where mods like MoreShipUpgrades allow LethalLib shop items to be disabled incorrectly.

## v0.1.4

- Fixed issues with checking unlockables and unlockables not registering.
- Fixed issues with trying to use the Achievement Predicate.
- Fixed Editor.dll issues with accessing achievements for AchievementTriggers script.
- Fixed issues with DailyPricingStrategy accessing TimeOfDay too early.
- Fixed issues with achievements causing a share violation error on loading multiple achievements simultaneously sometimes.

## v0.1.2

- Disabled achievements button if there are no achievements.

## v0.1.1

- Fixed some editor dll stuff and separated compatiblity with soundapi into another dll.

## v0.1.0

Hello World!

## DawnLib

- Added registering content through `DawnLib`
- Added `WeightTable` and `CurveTable` so that weights can be updated dynamically in-game.
- Added `ITerminalPurchasePredicate` to allow some items to not be bought based on conditions.
  - `TerminalPurchaseResult.Success()`: Acts like normal
  - `TerminalPurchaseResult.Failed()`: Player is unable to purchase the item and a different terminal node is displayed. Also allows overriding the name in this state with `.SetOverrideName(string)`
  - `TerminalPurchaseResult.Hidden()`: Does not show up in `store`, and player is also unable to purchase the item.
- `ITerminalPurchase` interface for (currently only some) possible terminal purchases:
  - `ITerminalPurchasePredicate Predicate`: see above, set with `builder.SetPredicate(predicate)`
  - `IProvider<int> Cost`: allows cost to be easily updated, set with `builder.SetCost(int)` or `builder.SetCost(provider)`
- Removed `VanillaEnemies` and added `LethalContent`
- Added `tags`
- Allow registering new tilesets to dungeons
- Added `PersistentDataContainer`

## DuskMod

**MIGRATING FROM CODEREBIRTHLIB:**

You will need to remake your `Content Container`, `Mod Information` and content definitions.
The general flow has not changed overall.

- Reworked `Content Container`:
  - `Entity Name` (from content definitions) and `Entity Name Reference` have been removed. Instead, content can just be referenced.
  - `Asset Bundle Name` is now a dropdown.
  - Button to dump all `NamespacedKey`s to a `.json` file to be used with the `TeamXiaolan.DawnLib.SourceGen` package.
- Added 4 types of `Achievements`: `Discovery`, `Instant`, `Stat`, `Parent`
- Content definitions can now apply `tags`
- Added `DuskModContent` for the `Achievement` registry.
- Moved `.RegisterMod()` from `CRLib` to `DuskMod`
- Added `DuskAdditionalTilesDefinition`
- Added 2 `DuskTerminalPredicate`s:
  - `ProgressivePredicate`: Allows for a shop item to be unlocked through progression
  - `AchievementPredicate`: Requires that the player purchasing the item has an achievement
- Added 1 `DuskPricingStrategy`:
  - **NOTE:** Setting a pricing strategy in a content definition *overrides* the price set in the config.
  - `DailyPricingStrategy`: Price of shop item changes as the quota progresses.

## Utilities / Misc

- Added `NetworkAudioSource`, `AssetBundleUtils`
- Removed `ExtendedLogging` and split it into `DebugLogSources` (yes this uses SoundAPI code)
