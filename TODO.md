# TODO list

## Xu

- ESR/MRR?
  - Transpilers
  - ParticleSystem replacements
  - Enemy Nests

- Vehicles

less important:

- Add a special finished border for each achievement mod.
- Add a normal border for each achievement mod.

## Bongo

- Check comment in dusk enemy replacement on the nest field.
- Rarity on the entity replacements, I think maybe it should work with like having the base default skin with a weight of 100 on All, and then other skins apply on top? im not sure but like some way to make it 100% snowy skins on snowy moons but also not having the snowy skins anywhere else and also having it allow multiple skins in one moon.

less important:

- Suits UI stuff for fitting em in.
- check over all visibility (public, internal, private)
- config stuff (NamespacedKey TOMLConverter, pretty print generics, look at patching to allow lists?)
- Create a "Terminal Store Catalogue" class that handles formatting to remove the need for the transpiler in TerminalPatches.cs
  - Would likely be a bit more incompatible with mods like TerminalFormatter in the beginning, but API should be open enough for them to adapt
- allow Dusk to set custom data
- DataContainer for lobby specific info? (things like gamemode settings, etc?)
- look at converting current info classes to be records
- NetworkDataContainer

## Needs to be done eventually
