using System.Collections.Generic;

namespace Dawn.SourceGen.Extensions;

public static class IEnumerableExtensions {
	public static IEnumerable<(int index, T value)> WithIndex<T>(this IEnumerable<T> list) {
		int i = 0;
		foreach(T data in list) {
			yield return (i, data);
			i++;
		}
	}
}