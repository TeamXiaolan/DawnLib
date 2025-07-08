# v0.7.2
- Improve LethalConfig Patch
  - It will no longer display (harmless) errors, and more configs will display inside LethalConfig.

## v0.7.1

- Forgot to add OwnerNetworkAnimator and ClientNetworkTransform.

## v0.7.0

- Added a bunch of random useful scripts like:
  - OwnerNetworkAnimator.
  - ClientNetworkTransform.
  - AmbientNoisePlayer
  - AutoRotate
  - ChanceScript
  - EnemyOnlyTriggers
  - PlayerOnlyTriggers
  - ForceScanColorOnItem
  - SpawnSyncedCRLibObject

## v0.6.0

- Fixed scraps and shop items not creating some configs.

## v0.5.0

- Fixed some exceptions with enemies that dont have an EnemyType, also added GiantKiwi to list of vanilla enemies.

## v0.4.1

- Fixed an issue with clients trying to spawn hazards they can't.

## v0.4.0

- Fixed a desync bug with spawning non networked outside hazards.
- Added more extended logging for potential.

## v0.3.1

- Probably fixed the last config issue with outside hazards?

## v0.3.0

- Fixed issues with config generation being both hard to read, not being generated at all due to other options or not generating the enabled config due to force enabling overriding it.
- Updated readme with a template for using CodeRebirthLib and how to setup a UnityProject for CodeRebirthLib (and also how you should be setting up a UnityProject for content like moons or anything you are doing really).

## v0.2.0

- Fixed WeatherRegistry SoftDependency not being so soft.

## v0.1.2

- Fixed reference of weather when recreating the Weather object in code, thanks morvian.
- Added nuget package.
- Fixed outside mapobject configs being labelled as inside and thus being overriden by inside spawn weight config generation.

## v0.1.1

- Finished adding Progressive Unlockables support.
- Fixed netcode patcher basically not working lol.
- Improved logging a lot.
- Added PathFindingLib as dependency because I forgot last version.
- Added LLL soft dependency for item and enemy ContentTag support.

## v0.1.0

- Added support for BoundedRange and AnimationCurve editor configs.
- Almost finished adding support for Progressive Unlockables.
- A lot of other things in preparation for usage by coderebirth (mod is currently able to be used by anyone it's just missing full progressive unlockable support and a nuget package).

## V0.0.1

- Initial Release.
