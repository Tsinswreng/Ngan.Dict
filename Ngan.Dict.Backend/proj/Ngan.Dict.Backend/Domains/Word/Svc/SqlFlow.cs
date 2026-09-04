using System.Runtime.CompilerServices;

namespace Ngan.Dict.Backend.Domains.Word.Svc;

/// 批原語原型(`_2` 鏈路的基石,按 2026-08-31 討論的方向先落在業務側試寫)。
/// 目標形態:批的編排權從業務手搓(BatchCollector + Add/End + 猜批大小)收歸原語,
/// 業務只聲明「一批做什麼」,批的切分/時機/尾批/攤平全部由本類承擔。
/// 設計成熟後整個類應下沉進 CsSql 成爲庫的一等公民(流切批器)、“SqlFlow”即預留名。
public static class SqlFlow
{
	/// 庫級默認批大小(與 BatchCollector.DfltBatchSize 同值——兩者都是過渡期的執行層策略,
	/// 設計上批大小應由庫按 DB/執行策略定,業務不許再出現批大小數字)。
	public static u64 DfltBatchSize{get;set;} = 100;

	/// 流切批器:把入參流按批消費,每攢夠一批就調 FnBatch(批內回調,不是再包流),
	/// 回調返回的結果按批順序攤平、出參流與入參流位置一一對應。
	/// 批大小 = 執行層策略(取本類 DfltBatchSize),業務不許再出現批大小數字;
	/// BatchSize 參數僅供測試/調權重覆蓋,等同討論稿中的 Opt。
	/// 空間複雜度 O(批大小)、與總元素數 n 無關;回調收到的 IList 是該批專屬、回調結束即棄。
	/// <typeparam name="TIn">入參元素類型</typeparam>
	/// <typeparam name="TOut">出參元素類型(回調內已可物化的結果元素)</typeparam>
	/// <param name="Src">入參流</param>
	/// <param name="FnBatch">一批做什麼:收 IList 批、回該批結果</param>
	/// <param name="Ct">取消令牌</param>
	/// <param name="BatchSize">批大小覆蓋(默認 null = 用庫級默認)</param>
	public static async IAsyncEnumerable<TOut> Batches<TIn, TOut>(
		IAsyncEnumerable<TIn> Src
		,Func<IList<TIn>, CT, Task<IEnumerable<TOut>>> FnBatch
		,[EnumeratorCancellation] CT Ct
		,u64? BatchSize = null
	){
		// 批大小收斂:null 或 0 都退回庫級默認,業務不硬編碼。
		var size = (BatchSize is null || BatchSize == 0) ? DfltBatchSize : BatchSize.Value;
		var buf = new List<TIn>((int)size);

		await foreach(var item in Src.WithCancellation(Ct)){
			buf.Add(item);
			// 滿批才執行:執行前先換新緩衝,回調即使持有 buf 引用也污染不了下一批。
			if((u64)buf.Count < size){
				continue;
			}
			var outs = await FnBatch(buf, Ct);
			buf = new List<TIn>((int)size);
			foreach(var o in outs){
				yield return o;
			}
		}

		// 尾批:不足一批的剩餘元素也要執行一次(位置對齊的最後一段)。
		if(buf.Count > 0){
			var outs = await FnBatch(buf, Ct);
			foreach(var o in outs){
				yield return o;
			}
		}
	}
}