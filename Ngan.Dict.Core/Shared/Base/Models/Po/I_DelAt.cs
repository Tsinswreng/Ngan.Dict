namespace Ngaq.Core.Shared.Base.Models.Po;

using System.Collections;
using System.Runtime.CompilerServices;
using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Tools;

public interface I_DelAt {
	public IdDel DelAt{get;set;}
}



public static class ExtnI_DelAt {
	extension<T>(T z)
		where T:I_DelAt
	{
	}

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
		await foreach (var ele in z) {
			if (ele.DelAt.IsNullOrDefault()) {
				yield return ele;
			}
		}
	}
}
