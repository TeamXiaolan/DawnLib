//HintName: some_test_filename.UnlockableItemKeys.g.cs
using Dawn;
#nullable enable
namespace My.Test.Namespace;
[System.CodeDom.Compiler.GeneratedCode("DawnLib", "<version scrubbed>")]
public static partial class UnlockableItemKeys {
	public static NamespacedKey<TestUnlockableItemInfo> Orangesuit = NamespacedKey<TestUnlockableItemInfo>.Vanilla("orange_suit");
	public static NamespacedKey<TestUnlockableItemInfo> Fridge = NamespacedKey<TestUnlockableItemInfo>.Vanilla("fridge");
	public static NamespacedKey<TestUnlockableItemInfo> NonVanillaNamespace = NamespacedKey<TestUnlockableItemInfo>.From("something", "else");
	public static NamespacedKey<TestUnlockableItemInfo>? GetByReflection(string name) {
		return (NamespacedKey<TestUnlockableItemInfo>?)typeof(UnlockableItemKeys).GetField(name)?.GetValue(null);
	}
}
