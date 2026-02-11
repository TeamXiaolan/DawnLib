# DawnLib Tests

## DawnLib.SourceGen.Tests

Automated test in `DawnLib.SourceGen.Tests` project can be executed with `dotnet test`. They use [Verify] snapshot testing tool.

[Verify]: https://github.com/VerifyTests/Verify

## DawnLib.SourceGen.IntegrationTests

Integration tests in `DawnLib.SourceGen.IntegrationTests` won't run without Unity netcode library, but can be built in CI.

Copy `Lethal Company_Data/Managed/Unity.Netcode.Runtime.dll` to `DawnLib/deps` directory in order to run tests.

Or use the following command to avoid running integration tests:

```sh
dotnet test --filter 'FullyQualifiedName!~.IntegrationTests'
```
