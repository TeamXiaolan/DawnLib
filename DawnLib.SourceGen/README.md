# TeamXiaolan.DawnLib.SourceGen

[![NuGet Version](https://img.shields.io/nuget/v/TeamXiaolan.DawnLib.SourceGen?logo=nuget)](https://www.nuget.org/packages?q=TeamXiaolan.DawnLib.SourceGen)

_Source generators to simplify DawnLib_

This package is a source code generator: a compiler extension that transforms
`*.namespaced_keys.json` and `*.tag.json` files into .NET classes with static
`NamespacedKey` members.

JSON data files must be included into the `<AdditionalFiles>` item group.

Classes are generated with the namespace defined by the `<RootNamespace>` of your _\*.csproj_.

## Example

For instance, given the following project setup:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <RootNamespace>Hello.World</RootNamespace>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="TeamXiaolan.DawnLib.SourceGen" Version="0.7.*" />
    </ItemGroup>

    <ItemGroup>
        <None Remove="data\**\*.json" />
        <AdditionalFiles Include="data\**\*.json" />
    </ItemGroup>
</Project>
```

`data/keys/foobar.namespaced_keys.json`:

```json
{
  "EnemyKeys": {
    "__type": "TestEnemyInfo",
    "Blob": "lethal_company:blob"
  }
}
```

`data/tags/abc_xyz.tag.json`:

```json
{
  "tag": "my_namespace:cold",
  "values": [
    "my_namespace:december",
    "my_namespace:january",
    "my_namespace:february"
  ]
}
```

The following code will be generated:

```cs
namespace Hello.World;

public static partial class EnemyKeys {
	public static NamespacedKey<TestEnemyInfo> Blob = NamespacedKey<TestEnemyInfo>.Vanilla("blob");

	public static NamespacedKey<TestEnemyInfo>? GetByReflection(string name) {
		return (NamespacedKey<TestEnemyInfo>?)typeof(EnemyKeys).GetField(name)?.GetValue(null);
	}
}

public static partial class Tags {
	public static NamespacedKey AbcXyz = NamespacedKey.From("my_namespace", "cold");
}
```
