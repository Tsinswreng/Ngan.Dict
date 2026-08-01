namespace Ngaq.Core.Shared.Word.WeightAlgo;

using System.Runtime.CompilerServices;
using Ngaq.Core.Word.Models;
using Ngaq.Core.Word.Models.Weight;
using Ngaq.Core.Word.Svc;
using Ngaq.Core.Word.WeightAlgo.Models;

using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Tools.Json;
using Tsinswreng.CsErr;
using Tsinswreng.CsTools;
using Ngaq.Core.Shared.Word.Models.Weight;
using Ngaq.Core.Shared.Word.WeightAlgo.Models;

[Doc(@$"默認內置權重算法")]
public partial class DfltWeightCalculator : IWeightCalctr {
	public static str Name=>"Tswg-EventFlowV1";
	
	public async Task<IWeightResult> Calc(
		IAsyncEnumerable<IWordForLearn> Words
		,IDictionary<str, obj?>? CalcArg
		,CT Ct
	){
		var cfg = new OptWeightResult {
			ResultType = EResultType.AsyEIWordWeightResult,
			SortBy = ESortBy.Weight
		};

		// Channel 容量決定背壓，可按實際調整
		var channel = Channel.CreateBounded<IWordWeightResult>(
			new BoundedChannelOptions(Environment.ProcessorCount * 2){//隊列最多能存嘰個結果對象
				SingleWriter = false, SingleReader = true
			}
		);

		// 並行度 == Environment.ProcessorCount 預設即可
		var parallelOptions = new ParallelOptions {
			CancellationToken = Ct,
			MaxDegreeOfParallelism = Environment.ProcessorCount//同時有嘰個綫程
		};

		// 把「計算」放到後台執行，並把異常透過 Channel 往消費端傳遞，避免 UI 端看到未處理崩潰。
		var producer = Task.Run(async () => {
			try {
				await Parallel.ForEachAsync(Words, parallelOptions, async (word, innerCt) => {
					// 快速剪枝
					if (word.PrevTurnLearnRecords.Count == 0 && word.Weight != null){
						return;
					}
					// 每個詞獨立 new CalculatorForOne，保證執行緒安全
					var calc = new CalculatorForOne();
					if(CalcArg is not null){
						try{
							calc.Cfg = DfltWeightCfg.FromDict(CalcArg);
						}catch(Exception e){
							throw KeysErr.Word.BuiltinWeightCalcArgParseFailed.ToErr()
								.AddErr(e)
								.AddDebugArgs(CalcArg);
						}
					}
					calc.Init(WeightWord.FromWordForLearn(word));
					await calc.RunAsy(innerCt);

					var result = new WordWeightResult {
						StrId = word.Id.ToString(),
						Weight = calc.WordState.Weight
					};
					await channel.Writer.WriteAsync(result, innerCt);
				});
				channel.Writer.TryComplete();
			}catch(OperationCanceledException oce){
				channel.Writer.TryComplete(oce);
			}catch(AppErr appErr){
				channel.Writer.TryComplete(appErr);
			}catch(Exception e){
				channel.Writer.TryComplete(
					KeysErr.Word.BuiltinWeightCalcExecFailed.ToErr()
					.AddErr(e)
				);
			}finally {
				// 若前面已 Complete，這裡 TryComplete 不會覆蓋原始異常。
				channel.Writer.TryComplete();
			}
		}, Ct);

		return new WeightResult {
			Opt = cfg,
			Results = ReadResultsAsyE(channel.Reader, producer, Ct),
		};
	}

	// public async Task<IWeightResult> CalcAsy(
	// 	IEnumerable<IWordForLearn> Words,
	// 	CT Ct
	// ){
	// 	return await CalcAsy(Words.ToAsyncEnumerable(), Ct);
	// }

	private static async IAsyncEnumerable<IWordWeightResult> ReadResultsAsyE(
		ChannelReader<IWordWeightResult> reader,
		Task Producer,
		[EnumeratorCancellation] CT Ct
	){
		// C# 迭代器限制：yield 不能放在帶 catch 的 try 裏。
		await foreach (var item in reader.ReadAllAsync(Ct)){
			yield return item;
		}

		// 結果流讀完後再等待生產者任務，補齊背景任務異常傳遞。
		try{
			await Producer;
		}catch(OperationCanceledException){
			throw;
		}catch(AppErr){
			throw;
		}catch(Exception e){
			throw KeysErr.Word.BuiltinWeightCalcExecFailed.ToErr()
				.AddErr(e);
		}
	}
}
