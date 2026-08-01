namespace Ngaq.Core.Shared.Word;

using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Word.Models.Weight;
using Ngaq.Core.Word.Svc;
using Tsinswreng.CsTools;
using Ngaq.Core.Frontend.User;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tsinswreng.CsErr;
using Ngaq.Core.Shared.Word.Models.Weight;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Core.Shared.Word.Models.Dto;
using System.Collections;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Shared.Word.WeightAlgo;

public enum ELearnOpRtn{
	Learn = 0
	,Undo = 1
	,Invalid = 2
}

public partial class OperationStatus{
	public bool Load = false;
	public bool Start = false;
	public bool Save = true;

}

public partial class MgrLearnedWords{
	public IDictionary<ELearn, IDictionary<IWordForLearn, nil>> Learn_WordSet{get;set;}
	= new Dictionary<ELearn, IDictionary<IWordForLearn, nil>>();

	protected IDictionary<IWordForLearn, nil> GetWordSet(ELearn Learn){
		if(Learn_WordSet.TryGetValue(Learn, out var WordSet)){
			return WordSet;
		}else{
			WordSet = new Dictionary<IWordForLearn, nil>();
			Learn_WordSet.Add(Learn, WordSet);
			return WordSet;
		}
	}

	public nil Set(
		ELearn Learn
		,IWordForLearn Word
	){
		var WordSet = GetWordSet(Learn);
		WordSet[Word] = NIL;
		return NIL;
	}

	public nil DeleteWordFromLearnGroup(
		ELearn Learn
		,IWordForLearn Word
	){
		var WordSet = GetWordSet(Learn);
		if(WordSet.ContainsKey(Word)){
			WordSet.Remove(Word);
		}
		return NIL;
	}

	public IEnumerable<IWordForLearn> GetLearnedWords(){
		foreach( var(Learn,WordSet) in Learn_WordSet){
			foreach( var(Word, _) in WordSet ){
				yield return Word;
			}
		}
	}


}

public partial class StateLearnWords{
	public IList<IWordForLearn> WordsToLearn { get; set; } = new List<IWordForLearn>();
	public MgrLearnedWords MgrLearnedWords { get; set; } = new MgrLearnedWords();
	public OperationStatus OperationStatus {get;set;} = new ();
}


public partial class LearnEventArgs :EventArgs{
	public IWordForLearn? Word{get;set;}
	public ELearn Learn{get;set;}
	public bool IsUndo{get;set;} = false;
}

/// 只會在前端運行
public partial class MgrLearn{
	//public MgrLearn(){}
	public ISvcWord SvcWord{get;set;}//TODO 接口隔離
	//IUserCtx UserCtx;
	IFrontendUserCtxMgr UserCtxMgr;
	IStudyPlanGetter StudyPlanGetter;
	public IWeightCalctr WeightCalctr{get;set;}
	public IDictionary<str, obj?> WeightArg{get;set;} = new Dictionary<str, obj?>();
	ILogger? Logger;
	public MgrLearn(
		ISvcWord SvcWord
		,IStudyPlanGetter StudyPlanGetter
		,IFrontendUserCtxMgr UserCtxMgr
		,ILogger? Logger
	){
		this.WeightCalctr = new DfltWeightCalculator();
		this.StudyPlanGetter = StudyPlanGetter;
		this.SvcWord = SvcWord;
		this.UserCtxMgr = UserCtxMgr;
		this.Logger = Logger;
	}

	public event EventHandler<LearnEventArgs>? OnLearnOrUndo;



	public partial class EvtArgOnErr:EventArgs{
		public object? Err{get;set;}
	}

	public event EventHandler<EvtArgOnErr>? OnErr;
	public object? LastErr{get;set;}
	public nil Err(object? Err){
		LastErr = Err;
		OnErr?.Invoke(this, new EvtArgOnErr{Err=Err});
		return NIL;
	}

	public StateLearnWords State{get;set;} = new();

	nil ResetLearnedState(){
		State.MgrLearnedWords = new();
		return NIL;
	}

	nil HandleLearnRecordsOnSave(){
		foreach(var Word in State.MgrLearnedWords.GetLearnedWords()){
			Word.HandleLearnRecordsOnSave();
		}
		return NIL;
	}

	public async Task<nil> LoadEtCalcWeight(
		IAsyncEnumerable<IJnWord> JnWords
		,CT Ct
	){
		try{
			// 每次重算前先清理上一輪錯誤，避免舊錯誤阻塞後續流程。
			LastErr = null;
			var z = this;
			var WordsForLearn = JnWords.Select(x=>new WordForLearn(x));
			var (weightCalctr, weightArg) = await GetCurWeightAlgo(Ct);
			var sw = Stopwatch.StartNew();
			var WeightResult = await weightCalctr.Calc(WordsForLearn, weightArg, Ct);
			sw.Stop();
			z.Logger?.LogInformation($"WeightCalctr.CalcAsy: {sw.ElapsedMilliseconds}ms");

			State.WordsToLearn.Clear();
			await foreach(var Word in WordsForLearn){
				State.WordsToLearn.Add(Word);
			}
			State.OperationStatus.Load = true;
			await HandleWeightResult(WeightResult, Ct);
		}catch(OperationCanceledException){
			throw;
		}catch(AppErr appErr){
			Err(appErr);
		}catch(Exception e){
			var appErr = KeysErr.Word.LoadWordCalcWeightFailed.ToErr();
			appErr.AddErr(e);
			appErr.AddDebugArgs("Phase=LoadEtCalcWeight");
			Err(appErr);
		}
		return NIL;
	}


	/// 取諸詞
	[Obsolete("用LoadEtCalcWeightAsy更快")]
	public nil Load(IEnumerable<IJnWord> JWords){
		State.WordsToLearn.Clear();
		foreach(var (i,JWord) in JWords.Index()){
			var Word = new WordForLearn(JWord);
			State.WordsToLearn.Add(Word);
		}
		State.OperationStatus.Load = true;
		return NIL;
	}

	public async Task<nil> CalcWeight(CT Ct){
		if(!State.OperationStatus.Load){
			return NIL;
		}
		try{
			LastErr = null;
			var (weightCalctr, weightArg) = await GetCurWeightAlgo(Ct);
			var WeightResult = await weightCalctr.Calc(State.WordsToLearn.ToAsyncEnumerable(), weightArg, Ct);
			await HandleWeightResult(WeightResult, Ct);
		}catch(OperationCanceledException){
			throw;
		}catch(AppErr appErr){
			Err(appErr);
		}catch(Exception e){
			var appErr = KeysErr.Word.WeightCalcRunFailed.ToErr();
			appErr.AddErr(e);
			appErr.AddDebugArgs("Phase=CalcWeight");
			Err(appErr);
		}

		return NIL;
	}

	async Task<(IWeightCalctr WeightCalctr, IDictionary<str, obj?> WeightArg)> GetCurWeightAlgo(CT Ct){
		try{
			var studyPlan = await StudyPlanGetter.GetStudyPlan(UserCtxMgr.GetUserCtx(), Ct);
			WeightCalctr = studyPlan.WeightCalctr ?? new DfltWeightCalculator();
			WeightArg = new Dictionary<str, obj?>();
			if(studyPlan.WeightArg is not null){
				foreach(var (k, v) in studyPlan.WeightArg){
					WeightArg[k] = v;
				}
			}
			return (WeightCalctr, WeightArg);
		}catch(OperationCanceledException){
			throw;
		}catch(AppErr){
			throw;
		}catch(Exception e){
			throw KeysErr.Word.WeightCalcGetStudyPlanFailed.ToErr()
				.AddErr(e);
		}
	}

	protected async Task<nil> HandleWeightResult(IWeightResult WeightResult, CT Ct){
		try{
			IDictionary<IdWord, IWordWeightResult> Id_Result;
			if(
				//WeightResult.Opt.ResultType == EResultType.ItblIWordWeightResult
				false
			){
				// var Result = (IEnumerable<IWordWeightResult>)WeightResult.Results!;
				// Id_Result = Result.ToDictionary(
				// 	x=>IdWord.FromLow64Base(x.StrId)
				// 	,x=>x
				// );
				Id_Result = new Dictionary<IdWord, IWordWeightResult>();
			}else{
				var Result = WeightResult.Results as IAsyncEnumerable<IWordWeightResult>;
				if(Result is null){
					throw KeysErr.Word.WeightCalcResultStreamNull.ToErr()
						.AddDebugArgs(WeightResult.Opt);
				}
				var dict = new Dictionary<IdWord, IWordWeightResult>();
				await foreach (var item in Result.WithCancellation(Ct)){
					IdWord key;
					try{
						key = IdWord.FromLow64Base(item.StrId);
					}catch(Exception e){
						throw KeysErr.Word.WeightCalcResultWordIdInvalid__.ToErr(item.StrId)
							.AddErr(e);
					}
					// 如果有重複 key 需求，自行決定是覆蓋還是拋異常
					dict[key] = item;          // 或 dict.TryAdd(key, item);
				}
				Id_Result = dict;
			}

			foreach(var Word in State.WordsToLearn){
				if(Id_Result.TryGetValue(Word.Id, out var Result)){
					Word.Weight = Result.Weight;
					Word.Index  = Result.Index;
				}
			}
			if(WeightResult.Opt.SortBy == ESortBy.Weight){
				State.WordsToLearn.Sort((b,a)=>(a.Weight??0).CompareTo(b.Weight));
			}else{
				State.WordsToLearn.Sort((a,b)=>(a.Index??0).CompareTo(b.Index));
			}
			u64 i = 0;
			foreach(var Word in State.WordsToLearn){
				Word.Index = i++;
			}
		}catch(OperationCanceledException){
			throw;
		}catch(AppErr){
			throw;
		}catch(Exception e){
			throw KeysErr.Word.WeightCalcResultHandleFailed.ToErr()
				.AddErr(e);
		}
		return NIL;
	}

	public async Task<nil> CalcWeightEtStart(CT Ct){
		if(!State.OperationStatus.Load){
			return NIL;
		}
		try{
			await CalcWeight(Ct);
		}catch(OperationCanceledException){
			throw;
		}catch(AppErr appErr){
			Err(appErr);
			return NIL;
		}catch(Exception e){
			var appErr = KeysErr.Word.StartLearnWithWeightCalcFailed.ToErr();
			appErr.AddErr(e);
			Err(appErr);
			return NIL;
		}
		// 算權重失敗時不啟動學習流程，避免 UI 進入錯誤狀態。
		if(LastErr is not null){
			return NIL;
		}
		await Start(Ct);
		return NIL;
	}

	public async Task<nil> Start(CT Ct){
		if(!State.OperationStatus.Load){
			return NIL;
		}
		State.OperationStatus.Start = true;
		return NIL;
	}


	ELearnOpRtn _Learn(
		IWordForLearn Word
		,ELearn Learn
	){
		if(!State.OperationStatus.Start){
			return ELearnOpRtn.Invalid;
		}
		var LearnRecord = new LearnRecord(Learn);
		var R = Word.AddLearnRecordIfEmpty(LearnRecord);
		State.OperationStatus.Save = false;
		if(R == 0){
			State.MgrLearnedWords.Set(Learn, Word);
			OnLearnOrUndo?.Invoke(this, new LearnEventArgs{Word=Word, Learn=Learn, IsUndo=false});
			return ELearnOpRtn.Learn;
		}
		return ELearnOpRtn.Undo;
	}

	ELearnOpRtn _Undo(
		IWordForLearn Word
	){
		if(!State.OperationStatus.Start){
			return ELearnOpRtn.Invalid;
		}
		var Last = Word.RmLastUnsavedLearnRecord();
		if(Last != null){
			State.MgrLearnedWords.DeleteWordFromLearnGroup(Last.Learn, Word);
			OnLearnOrUndo?.Invoke(this, new LearnEventArgs{Word=Word, Learn=Last.Learn, IsUndo=true});
			return ELearnOpRtn.Undo;
		}
		return ELearnOpRtn.Invalid;
	}


	public ELearnOpRtn LearnOrUndo(
		IWordForLearn Word
		,ELearn Learn
	){
		if(!State.OperationStatus.Start){
			return ELearnOpRtn.Invalid;
		}
		if(_Learn(Word, Learn) != ELearnOpRtn.Learn){
			return _Undo(Word);
		}
		return ELearnOpRtn.Learn;
	}

	public async Task<nil> Save(CT Ct){
		try{
			var UserCtx = UserCtxMgr.GetUserCtx();
			if(!State.OperationStatus.Start){
				return NIL;
			}
			var LearnedWord = State.MgrLearnedWords.GetLearnedWords();
			var WordId_LearnRecordss = LearnedWord.Select(x=>{
				var R = new WordId_LearnRecords();
				R.WordId = x.Id;
				R.LearnRecords = x.UnsavedLearnRecords;
				return R;
			});
			await SvcWord.AddWordId_LearnRecordss(
				UserCtx
				,WordId_LearnRecordss
				,Ct
			);
			HandleLearnRecordsOnSave();
			ResetLearnedState();
			State.OperationStatus.Save = true;
		}
		catch (Exception e){
			var E = KeysErr.Word.SaveWordListFailed.ToErr();
			E.AddErr(e);
			Err(E);
		}
		return NIL;
	}

	public nil Reset(){
		//ResetLearnedState();
		State = new();
		return NIL;
	}
}


