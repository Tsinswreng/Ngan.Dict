namespace Ngaq.Ui.Views.Word.Learn;

using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Ngaq.Core.Frontend.ImgBg;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Shared.Word;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordCard;
using Ngaq.Ui.Views.Word.WordInfo;
using Tsinswreng.CsCfg;
using Tsinswreng.CsTools;
using Ctx = VmLearnWords;

public partial class VmLearnWords
	:ViewModelBase
	,IWordCardMenuAction
{

	public partial class Cfg_{

		/// 單詞條長按

		public i64 LongPressDurationMs = 200;
		public IBrush ColorDflt = Brushes.Black;
		public IBrush ColorNone = Brushes.Transparent;
		// public IBrush ColorRmb = new SolidColorBrush(Color.FromArgb((u8)(0.8*0xff), 0, 80, 0));
		// public IBrush ColorFgt = new SolidColorBrush(Color.FromArgb((u8)(0.8*0xff), 80, 0, 0));
		public IBrush ColorRmb = new SolidColorBrush(Color.FromArgb((u8)(1*0xff), 0, 0xff, 0));
		public IBrush ColorFgt = new SolidColorBrush(Color.FromArgb((u8)(1*0xff), 0xff, 0, 0));
	}

	public Cfg_ CfgUi = new();
	ISvcWordV2 SvcWordV2 = default!;
	IFrontendUserCtxMgr UserCtxMgr = default!;

	MgrLearn MgrLearn = default!;
	IImgGetter SvcImg = default!;
	ICfgAccessor Cfg = default!;
	IWordCardPronounceBiz? WordCardPronounceBiz = default!;
	public VmLearnWords(
		ISvcWord SvcWord
		,ISvcWordV2 SvcWordV2
		,IFrontendUserCtxMgr UserCtxMgr
		,MgrLearn MgrLearn
		,IImgGetter SvcImg
		,ICfgAccessor Cfg
		,IWordCardPronounceBiz? WordCardPronounceBiz
	){
		this.SvcImg = SvcImg;
		this.SvcWordV2 = SvcWordV2;
		this.UserCtxMgr = UserCtxMgr;
		this.MgrLearn = MgrLearn;
		this.Cfg = Cfg;
		this.WordCardPronounceBiz = WordCardPronounceBiz;
		CurWordInfo.SetPromptBeforeStart();
		_Init();
		IsInited = true;
	}

	nil _Init(){
		_InitLearnMgr();
		_InitAutoPronounceListener();
		return NIL;
	}
	nil _InitLearnMgr(){
		if(AnyNull(MgrLearn)){return NIL;}
		MgrLearn.OnErr += (s,e)=>{
			HandleErr(e.Err);
		};
		MgrLearn.OnLearnOrUndo += (s,e)=>{
			OnPropertyChanged(nameof(IsSaved));
			ChangeBg().ContinueWith(t=>{
				if(t.IsFaulted){
					HandleErr(t.Exception);
				}
			});
		};
		return NIL;
	}

	/// 單詞卡片點擊事件（供業務監聽，避免把邏輯寫死在按鈕回調中）。
	public event Action<VmWordListCard>? OnWordCardClicked;

	/// 自動朗讀結果事件，由 View 層監聽後決定提示/導航。
	public event Action<DtoWordCardPronounceResult>? OnAutoPronounceResult;

	nil _InitAutoPronounceListener(){
		OnWordCardClicked += (Vm)=>{
			_ = HandleAutoPronounceOnWordCardClick(Vm, default);
		};
		return NIL;
	}

	async Task<nil> HandleAutoPronounceOnWordCardClick(VmWordListCard Vm, CT Ct){
		var enableAutoPronounce = Cfg.Get(KeysClientCfg.Word.EnableAutoPronounce);
		if(!enableAutoPronounce){
			return NIL;
		}
		var result = await PronounceWord(Vm?.WordForLearn?.JnWord, Ct);
		return NIL;
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmLearnWords(){
		#if false//||DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
			for(var i = 0; i < 20; i++){
				o.WordCards.Add(VmWordListCard.Samples[0]);
				o.WordCards.Add(VmWordListCard.Samples[1]);
			}
		}
		#endif
	}


	public ObservableCollection<VmWordListCard> WordCards{
		get;
		set{SetProperty(ref field, value);}
	} = new();


	public VmWordInfo CurWordInfo{
		get;
		set{SetProperty(ref field, value);}
	} = new();


	protected ELearnOpRtn _LearnOrUndo(VmWordListCard Vm, ELearn Learn){
		if(AnyNull(MgrLearn)){return ELearnOpRtn.Invalid;}
		if(Vm.WordForLearn == null){
			return ELearnOpRtn.Invalid;
		}
		var R = MgrLearn.LearnOrUndo(Vm.WordForLearn, Learn);
		SetCurVmWord(Vm);
		return R;
	}

	public nil ClickWordCard(VmWordListCard Vm){
		var CurLearnRecord = Vm.WordForLearn?.GetLastUnsavedLearnRecord();
		if(CurLearnRecord == null){
			//->Rmb
			_LearnOrUndo(Vm, ELearn.Rmb);
			Vm.LearnedColor = CfgUi.ColorRmb;
		}else if(CurLearnRecord.Learn == ELearn.Rmb){
			//-> Fgt
			_LearnOrUndo(Vm, ELearn.Fgt);//Make it undo
			_LearnOrUndo(Vm, ELearn.Fgt);//Make it be fgt
			Vm.LearnedColor = CfgUi.ColorFgt;
		}else if(CurLearnRecord.Learn == ELearn.Fgt){
			//->Clear
			_LearnOrUndo(Vm, ELearn.Fgt);//Make it undo
			Vm.LearnedColor = CfgUi.ColorNone;
		}
		OnWordCardClicked?.Invoke(Vm);
		return NIL;
	}

	/// 卡片菜單朗讀：先以 UserLang 找 NormLang，再調用 TTS 播放。
	public async Task<DtoWordCardPronounceResult> PronounceWord(IJnWord? JnWord, CT Ct){
		if(AnyNull(WordCardPronounceBiz, UserCtxMgr)){
			return new DtoWordCardPronounceResult{
				Status = EWordCardPronounceStatus.ServiceUnavailable,
			};
		}
		return await WordCardPronounceBiz.PronounceWord(
			UserCtxMgr.GetDbUserCtx(),
			JnWord,
			Ct
		);
	}

	public nil SetCurVmWord(VmWordListCard Vm){
		if(Vm.WordForLearn == null){
			return NIL;
		}
		CurWordInfo.FromIWordForLearn(Vm.WordForLearn);
		return NIL;
	}

	void _AssignWeightArg(){
		if(AnyNull(MgrLearn)){
			return;
		}
	}

	public async Task<nil> LoadEtStartAsy(CT Ct){
		var sw = Stopwatch.StartNew();
		_AssignWeightArg();
		await Task.Run(async()=>{
			if(!MgrLearn.State.OperationStatus.Load){
				//須先DBʹ詞ˇ全載入內存後交予MgrLearn。否則算權重旹併發讀則使Sqlite出錯
				var wordsAsyE = SvcWordV2.GetWordsToLearn(UserCtxMgr.GetDbUserCtx(), Ct);
				var sw2 = Stopwatch.StartNew();
				var wordList = await wordsAsyE.ToListAsync(Ct);
				sw2.Stop();
				LogInfo($"{nameof(SvcWordV2)}.{nameof(SvcWordV2.GetWordsToLearn)}: {sw2.ElapsedMilliseconds} ms");
				await MgrLearn.LoadEtCalcWeight(ToolAsyE.ToAsyE(wordList), Ct);
			}
			await MgrLearn.Start(Ct);
		}, Ct);
		RenderWordList();
		// After starting learning but before any word clicks
		CurWordInfo.SetPromptAfterStart();
		sw.Stop();
		LogInfo($"LoadEtStartAsy: {sw.ElapsedMilliseconds} ms");
		return NIL;
	}

	public async Task<nil> SaveEtRestart(CT Ct){
		_AssignWeightArg();
		await MgrLearn.Save(Ct);
		await MgrLearn.CalcWeightEtStart(Ct);
		RenderWordList();
		OnPropertyChanged(nameof(IsSaved));
		// Dispatcher.UIThread.Post(()=>{
		// 	ShowMsg($"SaveEtRestartAsy: {t1}ms, CalcWeightEtStartAsy: {t2}ms, RenderWordList: {t3}ms");
		// });
		return NIL;
	}

	public nil RenderWordList(){
		WordCards.Clear();
		var MaxDisplayedWordCount = Cfg?.Get(KeysClientCfg.Word.MaxDisplayedWordCount)??50;
		MgrLearn.State.WordsToLearn.Select(x=>{
			if((u64)WordCards.Count < MaxDisplayedWordCount){
				var vm = new VmWordListCard();
				vm.InitFromIWordForLearn(x);
				WordCards.Add(vm);
			}
			return 0;
		}).ToList();
		return NIL;
	}

	public async Task<nil> Reset(CT Ct){
		//MgrLearn = App.GetSvc<MgrLearn>();
		BgBrush = CfgUi.ColorDflt;
		MgrLearn.Reset();
		WordCards = new();
		// Reset 後列表為空，右側資訊區應回到學習提示，而不是殘留上一個單詞。
		CurWordInfo.SetPromptBeforeStart();
		return NIL;
	}

	public IBrush BgBrush{
		get;
		set{SetProperty(ref field, value);}
	} = Brushes.Black;

	public bool IsSaved{
		//get{return MgrLearn.State.MgrLearnedWords.GetLearnedWords().Any(w => w.UnsavedLearnRecords.Any());}
		get{return MgrLearn.State.OperationStatus.Save;}
	}


	private readonly ConcurrentQueue<Bitmap> _bgCache = new();

	public async Task<nil> ChangeBg(){
		CheckInit();
		try{
			var EnableRandomBackground = Cfg.Get(KeysClientCfg.Word.EnableRandomBackground);
			if(!EnableRandomBackground){
				return NIL;
			}
			await Task.Run(()=>{
	/* 1. 缓存有就拿一张用 */
				if (_bgCache.Count > 0){
					Dispatcher.UIThread.Post(()=>{
						if(_bgCache.TryDequeue(out var bm)){
							BgBrush = new ImageBrush(bm) { Stretch = Stretch.UniformToFill };
						}
					});
				}
				/* 2. 缓存没货才抓一批（第一次或异常后） */
				else{
					if (SvcImg == null) return NIL;
					var batch = SvcImg.GetN(3);
					if (batch == null) return NIL;
					int idx = 0;
					foreach (var obj in batch){
						if (obj.Type != typeof(Stream) || obj.Data == null) continue;
						using Stream s = (Stream)obj.Data;
						var bmp = new Bitmap(s);
						if (idx == 0){// 第一张直接上屏
							Dispatcher.UIThread.Post(()=>{
								BgBrush = new ImageBrush(bmp) { Stretch = Stretch.UniformToFill };
							});
						}else{// 其余先囤着
							_bgCache.Enqueue(bmp);
						}
						idx++;
					}
				}

				/* 3. 用掉一张立刻补一张，保持库存 2 张 */
				if (_bgCache.Count < 2 && SvcImg != null){
					var obj = SvcImg.GetN(1).FirstOrDefault();
					if (obj != null && obj.Type == typeof(Stream) && obj.Data != null){
						using (Stream s = (Stream)obj.Data){
							_bgCache.Enqueue(new Bitmap(s));
						}
					}
				}
				return NIL;
			});
		}
		catch (Exception e){
			LogError(e.ToString());
		}
		return NIL;
	}


}
