namespace Ngaq.Core.Tools{
using Num = i64;
public partial class ToolRandom {
	private static readonly Random rnd = new Random();

	
	/// 生成随机整数数组，可以指定是否允许重复
	
	/// <param name="min">范围最小值（包含）</param>
	/// <param name="max">范围最大值（包含）</param>
	/// <param name="howMany">生成数量</param>
	/// <param name="allowDuplicate">是否允许重复，默认允许</param>
	/// <returns>随机整数列表</returns>
	public static List<Num> RandomArrI64(Num min, Num max, u64 howMany, bool allowDuplicate = true) {
		if (!allowDuplicate) {
			return NonDuplicateI64(min, max, howMany);
		} else {
			return DuplicateI64(min, max, howMany);
		}
	}

	// 允许重复的随机数生成
	private static List<Num> DuplicateI64(Num min, Num max, u64 howMany) {
		List<Num> result = new List<Num>((i32)howMany);
		for (u64 i = 0; i < howMany; i++) {
			// Random.Next(min, max + 1) 包含 min，最大不包含 max + 1，即包含 max
			int value = rnd.Next((i32)min, (i32)max + 1);
			result.Add(value);
		}
		return result;
	}

	// 不允许重复的随机数生成（类似 Fisher-Yates shuffle）
	private static List<Num> NonDuplicateI64(Num min, Num max, u64 howMany) {
		var rangeLength = max - min + 1;
		if ((u64)rangeLength < howMany) {
			throw new ArgumentOutOfRangeException(nameof(howMany), "范围内没有足够的数字满足不重复的需求");
		}

		// 创建范围数组
		List<Num> integerList = new List<Num>((i32)rangeLength);
		for (var i = min; i <= max; i++) {
			integerList.Add(i);
		}

		// Fisher-Yates 洗牌算法，打乱数组
		for (var i = rangeLength - 1; i > 0; i--) {
			int j = rnd.Next((i32)i + 1);
			var ii = (i32)i;
			var temp = integerList[ii];
			integerList[ii] = integerList[j];
			integerList[j] = temp;
		}

		// 返回前 howMany 个元素
		return integerList.GetRange(0, (i32)howMany);
	}
}


}


#region u64
namespace Ngaq.Core.Tools{
using Num = u64;
public partial class ToolRandom {
	//private static readonly Random rnd = new Random();

	
	/// 生成随机整数数组，可以指定是否允许重复
	
	/// <param name="min">范围最小值（包含）</param>
	/// <param name="max">范围最大值（包含）</param>
	/// <param name="howMany">生成数量</param>
	/// <param name="allowDuplicate">是否允许重复，默认允许</param>
	/// <returns>随机整数列表</returns>
	public static List<Num> RandomArrU64(Num min, Num max, u64 howMany, bool allowDuplicate = true) {
		if (!allowDuplicate) {
			return NonDuplicateU64(min, max, howMany);
		} else {
			return DuplicateU64(min, max, howMany);
		}
	}

	// 允许重复的随机数生成
	private static List<Num> DuplicateU64(Num min, Num max, u64 howMany) {
		List<Num> result = new List<Num>((i32)howMany);
		for (u64 i = 0; i < howMany; i++) {
			// Random.Next(min, max + 1) 包含 min，最大不包含 max + 1，即包含 max
			var value = (u64)rnd.Next((i32)min, (i32)max + 1);
			result.Add(value);
		}
		return result;
	}

	// 不允许重复的随机数生成（类似 Fisher-Yates shuffle）
	private static List<Num> NonDuplicateU64(Num min, Num max, u64 howMany) {
		var rangeLength = max - min + 1;
		if ((u64)rangeLength < howMany) {
			throw new ArgumentOutOfRangeException(nameof(howMany), "范围内没有足够的数字满足不重复的需求");
		}

		// 创建范围数组
		List<Num> integerList = new List<Num>((i32)rangeLength);
		for (var i = min; i <= max; i++) {
			integerList.Add(i);
		}

		// Fisher-Yates 洗牌算法，打乱数组
		for (var i = rangeLength - 1; i > 0; i--) {
			int j = rnd.Next((i32)i + 1);
			var ii = (i32)i;
			var temp = integerList[ii];
			integerList[ii] = integerList[j];
			integerList[j] = temp;
		}

		// 返回前 howMany 个元素
		return integerList.GetRange(0, (i32)howMany);
	}
}


}
#endregion u64

