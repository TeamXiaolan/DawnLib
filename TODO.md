# TODO list

## Needs to be done

- Enemy bestiaries.
- Use FailedTerminalResult.OverrideName
- Tag weights
- MapObject CRInfo stuff?
- Weather Namespace shenanigans with CRLib created weathers.
- Figure out the interior name situation with weights.
- editor validation? ex:
  - empty config name
  - things like null references in content definitions?
  - no references across asset bundles

## Needs to be done eventually

- Suits
- ContentContainer each content element shows up as `element {x}` rather than the entity name reference/name of the content
- Enemies have an Inside, Outside and Daytime weights in the info regardless of it.
- ScanNode having preloader stuff to save its equivalent recttransform
- Swap TerminalPredicate stuff with preloader stuff.
- Add a special finished border for each achievement mod.
- Add a normal border for each achievement mod.

### Code cleanup
- Use `const int` to describe `order` for `CreateAssetMenu`s