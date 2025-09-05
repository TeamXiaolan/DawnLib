# TODO list

## Xu

- Add a special finished border for each achievement mod.
- Add a normal border for each achievement mod.
- configs for pricing strategies (DuskItemDefinition, DuskUnlockableDefinition)

## Bongo

- Figure out the interior name situation with weights.

less important:

- Generalise the Collecting of LLL tags for each registry.
- check over all visibility (public, internal, private)
- config stuff (NamespacedKey TOMLConverter, pretty print generics, look at patching to allow lists?)
- Create a "Terminal Store Catalogue" class that handles formatting to remove the need for the transpiler in TerminalPatches.cs
  - Would likely be a bit more incompatible with mods like TerminalFormatter in the beginning, but API should be open enough for them to adapt

## Needs to be done eventually

- Suits
- ESR/MRR?
- Vehicles
- Swap tags being stored in lists to being stored in hash sets.
- get RequestNode, ConfirmNode for DawnUnlockableInfo
