//HintName: some_test_filename.EnemyKeys.g.cs
using Dawn;
#nullable enable
namespace My.Test.Namespace;
[System.CodeDom.Compiler.GeneratedCode("DawnLib", "<version scrubbed>")]
public static partial class EnemyKeys {
	public static NamespacedKey<TestEnemyInfo> Blob = NamespacedKey<TestEnemyInfo>.Vanilla("blob");
	public static NamespacedKey<TestEnemyInfo>? GetByReflection(string name) {
		return (NamespacedKey<TestEnemyInfo>?)typeof(EnemyKeys).GetField(name)?.GetValue(null);
	}
}
