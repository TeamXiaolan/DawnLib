# TODO list

## Xu

- DawnLib editor dll that's exclusively for stuff like LLL to DawnLib converter tool.
- Dead body additions.
- Handling vehicle spawning when already bought on loading a lobby, also always have the station spawn at the start of the lobby.
- Handling vehicle no station equaling magnet station.

less important:

- Add acceptable value ranges on the editor configs.
- Add a special finished border for each achievement mod.
- Add a normal border for each achievement mod.

## Bongo

- Hotloading interiors, Xu has a good idea for it.
  - Load the interior bundle after pulling lever and deciding which interior it is.
  - Register the network prefabs before any SpawnSyncedObject stuff loads.
  - Unregister network prefabs and Unload the interior bundle as soon as moon scene unloaded.
- Save system using lookup table with IDs to be slightly faster/more efficient.

less important:

- Config grabbing in AssetBundleLoader should be more optimised.
- Suits UI stuff for fitting em in.
- check over all visibility (public, internal, private)
- config stuff (NamespacedKey TOMLConverter, pretty print generics, look at patching to allow lists?)
- Create a "Terminal Store Catalogue" class that handles formatting to remove the need for the transpiler in TerminalPatches.cs
  - Would likely be a bit more incompatible with mods like TerminalFormatter in the beginning, but API should be open enough for them to adapt
- allow Dusk to set custom data
- DataContainer for lobby specific info? (things like gamemode settings, etc?)
- look at converting current info classes to be records
- NetworkDataContainer
- switch to using composition over inheritance (e.g. ItemUpgradeScrap, MoonUnlockScrap whatever)

## Needs to be done eventually
