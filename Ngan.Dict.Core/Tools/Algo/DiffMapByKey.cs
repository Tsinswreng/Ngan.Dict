namespace Ngaq.Core.Tools.Algo;
public partial class Algo{
/**
 * 取差集 支持 Map 和 Set(暫不支持)
 * 比较两个 Map，并返回一个新的 Map，该 Map 包含在第一个 Map 中但不在第二个 Map 中的键值对。
 * @param map1
 * @param map2
 * @returns
 */
	public static Dictionary<K, V> DiffMapByKey<K, V>(
		Dictionary<K, V> map1
		, Dictionary<K, V> map2
	)where K:notnull
	{

		/*
	const ans = new Map<K, V>();
	map1.forEach((value, key) => {
		if (!map2.has(key)) {
			ans.set(key, value);
		}
	});
	return ans;
		 */
		return map1
			.Where(kvp => !map2.ContainsKey(kvp.Key))
			.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	}
}
