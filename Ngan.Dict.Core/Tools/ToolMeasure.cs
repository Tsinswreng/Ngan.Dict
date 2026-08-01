namespace Ngaq.Core.Tools;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

public static class ExtnTiming {
	public static TRet Time<TRet>(this Func<TRet> a, [CallerMemberName] string name = "") {
		var sw = Stopwatch.StartNew();
		var R = a();
		sw.Stop();
		Console.WriteLine($"{name} 耗时 {sw.ElapsedMilliseconds} ms");
		return R;
	}

	
	/// 不能直接給lamda字面量用擴展方法
	public static async Task<TRet> TimeAsy<TRet>(
		this Func<Task<TRet>> Fn
		,ILogger Logger
		,[CallerMemberName] string name = ""
	) {
		//System.Console.WriteLine($">in {name}");
		var sw = Stopwatch.StartNew();
		var R = await Fn();
		sw.Stop();
		//Console.WriteLine($"<in {name} 耗时 {sw.ElapsedMilliseconds} ms");
		Logger.LogInformation($"{name} 耗時 {sw.ElapsedMilliseconds} ms");
		return R;
	}

	//測不準 總得0
	// public static async Task<(TRet, i64)> TimeAsy<TRet>(
	// 	this Task<TRet> z
	// ){
	// 	var sw = Stopwatch.StartNew();
	// 	var R = await z;
	// 	sw.Stop();
	// 	return (R, sw.ElapsedMilliseconds);
	// }

	// public static async Task<TRet> TimeAsy<TRet>(this Task<TRet> a, [CallerMemberName] string name = "") {
	// 	var sw = Stopwatch.StartNew();
	// 	var R = await a;
	// 	sw.Stop();
	// 	Console.WriteLine($"{name} 耗时 {sw.ElapsedMilliseconds} ms");
	// 	return R;
	// }

}
