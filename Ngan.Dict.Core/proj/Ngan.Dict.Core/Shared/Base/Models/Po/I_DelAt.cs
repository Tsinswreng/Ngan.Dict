namespace Ngan.Dict.Core.Shared.Base.Models.Po;

using System.Runtime.CompilerServices;
using Tsinswreng.CsCore;
using Tsinswreng.CsEntity;

// 軟刪列的介面本身由 CsEntity 提供（Tsinswreng.CsEntity.I_DelAt，DelAt 為 UnixMs、0 表示未刪），
// 本檔只留本項目慣用的「過濾未刪」擴展。

[Doc($$"""
對 {{nameof(I_DelAt)}} 的聚合做「只留未刪」的投影。

*為何是投影而不是原地刪除*：調用方拿到的可能是共享集合或流，這裡一律用惰性過濾，
不改變來源、也不把整個集合載入內存。
""")]
public static class ExtnI_DelAt {
	public static IEnumerable<T> FilterNonDel<T>(
		this IEnumerable<T> z
	)where T:I_DelAt
	{
		foreach (var ele in z) {
			if (ele.DelAt.IsNullOrDefault()) {
				yield return ele;
			}
		}
	}

	public static async IAsyncEnumerable<T> FilterNonDel<T>(
		this IAsyncEnumerable<T> z
		,[EnumeratorCancellation]
		CT Ct
	)where T:I_DelAt
	{
		await foreach (var ele in z.WithCancellation(Ct)) {
			if (ele.DelAt.IsNullOrDefault()) {
				yield return ele;
			}
		}
	}
}
