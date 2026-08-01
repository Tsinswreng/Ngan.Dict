namespace Ngaq.Ui.Views.Dictionary;

using Avalonia.Controls;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Dictionary.Svc;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Dictionary.SimpleWord;
using Ngaq.Ui.Views.Dictionary.NormLangTag;
using Ngaq.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangEdit;
using Tsinswreng.CsErr;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.CsTools;
using Tsinswreng.CsCfg;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

using Ctx = VmDictionary;

public partial class VmDictionary{
	public static partial Ctx Mk(){
		return new Ctx();
	}

	public partial nil SwapLang(){
		var tmp = SrcLang;
		SrcLang = TgtLang;
		TgtLang = tmp;
		var tmpName = SrcLangTranslatedName;
		SrcLangTranslatedName = TgtLangTranslatedName;
		TgtLangTranslatedName = tmpName;
		_ = RefreshLangTranslatedNames(default);
		_ = PersistCurLangs(default);
		return NIL;
	}

	public partial nil ApplySrcNormLang(PoNormLang Po){
		SrcLang = Po.Code ?? "";
		_ = RefreshLangTranslatedNames(default);
		_ = PersistSrcLang(Po, default);
		return NIL;
	}

	public partial nil ApplyTgtNormLang(PoNormLang Po){
		TgtLang = Po.Code ?? "";
		_ = RefreshLangTranslatedNames(default);
		_ = PersistTgtLang(Po, default);
		return NIL;
	}

	public async partial Task<nil> InitLang(CT Ct){
		if(AnyNull(SvcDictionary, FrontendUserCtxMgr)){
			return NIL;
		}
		try{
			var dbCtx = FrontendUserCtxMgr.GetDbUserCtx();
			var src = await SvcDictionary.GetCurSrcNormLang(dbCtx, Ct);
			var tgt = await SvcDictionary.GetCurTgtNormLang(dbCtx, Ct);
			if(src is not null && !str.IsNullOrWhiteSpace(src.Code)){
				SrcLang = src.Code;
			}
			if(tgt is not null && !str.IsNullOrWhiteSpace(tgt.Code)){
				TgtLang = tgt.Code;
			}
			await RefreshLangTranslatedNames(Ct);
		}catch(Exception ex){
			HandleErr(ex);
		}
		return NIL;
	}

	public async partial Task<nil> Lookup(CT Ct){
		if(string.IsNullOrWhiteSpace(Input)){
			return NIL;
		}
		if(AnyNull(SvcDictionary, FrontendUserCtxMgr, SvcNormLang)){
			return NIL;
		}
		var User = FrontendUserCtxMgr.GetUserCtx();
		// 先快照當前界面數據：若生成失敗則回滾，避免「內容被清空」。
		var PrevResult = CaptureResultSnapshot(Result);
		var PrevReq = LastReqLlmDict;
		var PrevResp = LastRespLlmDict;
		var PrevRawOutput = LastLlmRawOutput;
		var StreamedResp = new StringBuilder();
		var PendingUiSeg = new StringBuilder();
		var streamLock = new object();
		var UiFlushQueued = new IntBox();

		HasLookupStarted = true;
		ResetQuickSaveState();
		Result ??= App.DiOrMk<VmSimpleWord>();
		Result.StartStreaming(Input.Trim());

		var ReqLang = await BuildReqOptLang(User.ToDbUserCtx(), Ct);
		var Req = new ReqLlmDictEvt{
			Query = new Query{
				Term = Input.Trim(),
			},
			OptLang = ReqLang,
			OnNewSeg = (dto, ct) => {
				if(dto.NewSeg is not null){
					lock(streamLock){
						StreamedResp.Append(dto.NewSeg);
						PendingUiSeg.Append(dto.NewSeg);
					}
				}
				QueueFlushStreamedSegToUi(PendingUiSeg, streamLock, UiFlushQueued);
				return 0;
			},
			OnDone = (dto, ct) => {
				return 0;
			},
		};
		// 緩存最近一次查詢上下文，便於後續轉換為 JnWord。
		LastReqLlmDict = Req;
		LastRespLlmDict = null;

		try{
			// 先記住流式階段已展示的文本；若最終結構化解析結果缺失描述，則回退到它。
			var StreamedDescription = Result.Description;
			var Resp = await SvcDictionary.Lookup(User, Req, Ct);
			lock(streamLock){
				LastLlmRawOutput = StreamedResp.ToString();
			}
			LogWarn(
				$"Dictionary raw output after stream lookup. " +
				$"{DescribeTextTail(nameof(LastLlmRawOutput), LastLlmRawOutput)}"
			);
			Result.FromRespLlmDict(Resp);
			if(
				str.IsNullOrWhiteSpace(Result.Description)
				&& !str.IsNullOrWhiteSpace(StreamedDescription)
			){
				Result.Description = StreamedDescription;
				LogWarn(
					$"Dictionary lookup finalized with empty description, fallback to streamed text. " +
					$"Input={Input.Trim()}, SrcLang={SrcLang}, TgtLang={TgtLang}"
				);
			}
			LastRespLlmDict = Resp;
			CanQuickSaveCurrentLookup = true;
		}catch(Exception ex){
			str streamedText;
			lock(streamLock){
				streamedText = StreamedResp.ToString();
			}
			if(IsLookupCanceled(ex, Ct)){
				HandleLookupCanceled(
					Req,
					PrevResult,
					PrevReq,
					PrevResp,
					PrevRawOutput,
					streamedText
				);
				return NIL;
			}
			var isParseFailed = IsLlmResponseParseFailed(ex);
			if(isParseFailed){
				// 解析失敗時保留流式已展示內容，並保留原始輸出供「查看/編輯原始響應」使用。
				LastReqLlmDict = Req;
				LastRespLlmDict = null;
				LastLlmRawOutput = streamedText;
				if(
					str.IsNullOrWhiteSpace(Result?.Description)
					&& !str.IsNullOrWhiteSpace(streamedText)
				){
					Result ??= App.DiOrMk<VmSimpleWord>();
					Result.Description = streamedText;
				}
				LogWarn(
					$"Dictionary lookup parse failed, keeping streamed content. " +
					$"Input={Input.Trim()}, SrcLang={SrcLang}, TgtLang={TgtLang}, " +
					$"LlmResponse={streamedText}"
				);
			}else{
				RestoreResultSnapshot(PrevResult);
				LastReqLlmDict = PrevReq;
				LastRespLlmDict = PrevResp;
				LastLlmRawOutput = PrevRawOutput;
				LogError(
					$"Dictionary lookup failed. " +
					$"Input={Input.Trim()}, SrcLang={SrcLang}, TgtLang={TgtLang}, " +
					$"LlmResponse={streamedText}"
				);
			}
			HandleErr(ex);
		}

		return NIL;
	}

	public async partial Task<nil> LoadSrcLangTags(CT Ct){
		var Cfgs = DictionarySrcLangTagCfg.Load(AppCfg.Inst);
		var PosByKey = new Dictionary<str, PoNormLang>();
		if(SvcNormLang is not null && FrontendUserCtxMgr is not null && Cfgs.Count > 0){
			try{
				var Query = ToolAsyE.ToAsyE(
					Cfgs.Select(X=>(X.Type, X.Code))
				);
				var Pos = SvcNormLang.OrdGetNormLangByTypeCode(
					FrontendUserCtxMgr.GetDbUserCtx(),
					Query,
					Ct
				);
				await foreach(var Po in Pos.WithCancellation(Ct)){
					if(Po is not null){
						PosByKey[SrcLangTagKey(Po.Type, Po.Code)] = Po;
					}
				}
			}catch(Exception Ex){
				HandleErr(Ex);
			}
		}

		var Tags = new ObservableCollection<VmNormLangTag>();
		foreach(var Cfg in Cfgs){
			PosByKey.TryGetValue(SrcLangTagKey(Cfg.Type, Cfg.Code), out var Po);
			Tags.Add(LoadSrcLangTag(Cfg, Po));
		}
		SrcLangTags = Tags;
		RefreshSrcLangTagSelection();
		return NIL;
	}

	public partial nil ApplySrcNormLangTag(VmNormLangTag Tag){
		if(Tag.IsLang(ELangIdentType.Bcp47, SrcLang)){
			return NIL;
		}
		ApplySrcNormLang(new PoNormLang{
			Type = Tag.Type,
			Code = Tag.Code,
			NativeName = Tag.NativeName,
		});
		return NIL;
	}

	public partial nil ReplaceSrcLangTags(IEnumerable<VmNormLangTag> Tags){
		SrcLangTags = new ObservableCollection<VmNormLangTag>(Tags);
		RefreshSrcLangTagSelection();
		return NIL;
	}

	private partial void QueueFlushStreamedSegToUi(
		StringBuilder PendingUiSeg,
		object StreamLock,
		IntBox UiFlushQueued
	){
		if(Interlocked.Exchange(ref UiFlushQueued.Value, 1) != 0){
			return;
		}
		Dispatcher.UIThread.Post(()=>{
			str MergedSeg;
			lock(StreamLock){
				MergedSeg = PendingUiSeg.ToString();
				PendingUiSeg.Clear();
			}

			// step 1: 每次 Post 只刷新當前這一批文本。
			// 之前這裡會在 UI 線程裏 while(true) 一次吃完整個 backlog，
			// 安卓上容易把 hover / click 等輸入事件一直排在後面，
			// 造成「解析明明結束了，但收藏按鈕還像卡死」的假象。
			if(!str.IsNullOrEmpty(MergedSeg)){
				Result?.GotNewSeg(new DtoOnNewSeg{
					NewSeg = MergedSeg,
				});
			}

			// step 2: 先釋放「已有刷新任務」標記，讓新到達的片段可自行排隊。
			Interlocked.Exchange(ref UiFlushQueued.Value, 0);

			// step 3: 若清空當前批次後又積壓了新片段，重新 Post 下一輪。
			// 這裡故意不在當前回調裏 while 連刷，避免長時間霸佔 UI 線程。
			lock(StreamLock){
				if(PendingUiSeg.Length == 0){
					return;
				}
			}
			QueueFlushStreamedSegToUi(PendingUiSeg, StreamLock, UiFlushQueued);
		});
	}

	private partial nil HandleLookupCanceled(
		IReqLlmDict Req,
		LookupResultSnapshot PrevResult,
		IReqLlmDict? PrevReq,
		IRespLlmDict? PrevResp,
		str PrevRawOutput,
		str StreamedText
	){
		Result?.StopStreaming();
		if(str.IsNullOrWhiteSpace(StreamedText)){
			RestoreResultSnapshot(PrevResult);
			LastReqLlmDict = PrevReq;
			LastRespLlmDict = PrevResp;
			LastLlmRawOutput = PrevRawOutput;
			LogInfo(
				$"Dictionary lookup canceled before first streamed segment. " +
				$"Input={Input.Trim()}, SrcLang={SrcLang}, TgtLang={TgtLang}"
			);
			return NIL;
		}

		LastReqLlmDict = Req;
		LastRespLlmDict = null;
		LastLlmRawOutput = StreamedText;
		if(Result is not null && str.IsNullOrWhiteSpace(Result.Description)){
			Result.Description = StreamedText;
		}
		LogInfo(
			$"Dictionary lookup canceled after partial streamed response. " +
			$"Input={Input.Trim()}, SrcLang={SrcLang}, TgtLang={TgtLang}, " +
			$"LlmResponse={StreamedText}"
		);
		return NIL;
	}

	public partial Task<nil> ReparseFromRawOutput(str RawOutput, CT Ct){
		if(str.IsNullOrWhiteSpace(RawOutput)){
			ShowDialog(I18n[K.RawOutputEmptyCannotParse]);
			return Task.FromResult<nil>(NIL);
		}
		if(AnyNull(SvcDictionary, FrontendUserCtxMgr)){
			return Task.FromResult<nil>(NIL);
		}
		var PrevResult = CaptureResultSnapshot(Result);
		var PrevReq = LastReqLlmDict;
		var PrevResp = LastRespLlmDict;
		var PrevRawOutput = LastLlmRawOutput;
		try{
			HasLookupStarted = true;
			LogWarn(
				$"Dictionary raw output before manual reparse. " +
				$"{DescribeTextTail(nameof(RawOutput), RawOutput)}"
			);
			var Req = BuildFallbackReqLlmDict();
			var Resp = SvcDictionary.ParseRawOutput(RawOutput);
			Result ??= App.DiOrMk<VmSimpleWord>();
			Result.FromRespLlmDict(Resp);
			if(str.IsNullOrWhiteSpace(Result.Description)){
				Result.Description = RawOutput;
			}
			LastReqLlmDict = Req;
			LastRespLlmDict = Resp;
			LastLlmRawOutput = RawOutput;
			CanQuickSaveCurrentLookup = true;
		}catch(Exception ex){
			RestoreResultSnapshot(PrevResult);
			LastReqLlmDict = PrevReq;
			LastRespLlmDict = PrevResp;
			LastLlmRawOutput = PrevRawOutput;
			LogError($"Dictionary reparse from raw output failed. RawOutput={RawOutput}");
			HandleErr(ex);
		}
		return Task.FromResult<nil>(NIL);
	}

	private async partial Task<OptLang> BuildReqOptLang(IDbUserCtx DbUserCtx, CT Ct){
		if(SvcNormLang is null){
			return BuildFallbackReqLlmDict().OptLang;
		}

		var Query = ToolAsyE.ToAsyE([
			(ELangIdentType.Bcp47, SrcLang),
			(ELangIdentType.Bcp47, TgtLang),
		]);
		PoNormLang? SrcPo = null;
		PoNormLang? TgtPo = null;
		var Index = 0;
		await foreach(var Po in SvcNormLang.OrdGetNormLangByTypeCode(DbUserCtx, Query, Ct).WithCancellation(Ct)){
			if(Index == 0){
				SrcPo = Po;
			}else if(Index == 1){
				TgtPo = Po;
			}
			Index++;
		}

		return new OptLang{
			SrcLang = ToNormLangDetail(SrcLang, SrcPo),
			TgtLangs = [ToNormLangDetail(TgtLang, TgtPo)],
		};
	}

	private partial ReqLlmDictEvt BuildFallbackReqLlmDict(){
		return new ReqLlmDictEvt{
			Query = new Query{
				Term = Input.Trim(),
			},
			OptLang = new OptLang{
				SrcLang = ToNormLangDetail(SrcLang, null),
				TgtLangs = [ToNormLangDetail(TgtLang, null)],
			},
		};
	}

	private static partial NormLangDetail ToNormLangDetail(str Code, PoNormLang? Po){
		return new NormLangDetail{
			Type = ELangIdentType.Bcp47,
			Code = Code,
			NativeName = Po?.NativeName ?? "",
			EnglishName = Po?.EnglishName ?? "",
		};
	}

	private partial LookupResultSnapshot CaptureResultSnapshot(VmSimpleWord? Cur){
		if(Cur is null){
			return new LookupResultSnapshot();
		}
		return new LookupResultSnapshot{
			Head = Cur.Head,
			Description = Cur.Description,
			Pronunciations = Cur.Pronunciations.Select(p=>new Pronunciation{
				TextType = p.TextType,
				Text = p.Text,
			}).ToList(),
		};
	}

	private partial nil RestoreResultSnapshot(LookupResultSnapshot Snapshot){
		Result ??= App.DiOrMk<VmSimpleWord>();
		Result.Head = Snapshot.Head;
		Result.Description = Snapshot.Description;
		Result.Pronunciations = Snapshot.Pronunciations;
		return NIL;
	}

	public async partial Task<nil> ToWordEdit(CT Ct){
		if(AnyNull(SvcWordV2, FrontendUserCtxMgr, LastReqLlmDict, LastRespLlmDict)){
			ShowToast(I18n[K.CompleteDictionaryQueryBeforeSave]);
			return NIL;
		}
		var Req = LastReqLlmDict!;
		var Resp = LastRespLlmDict!;
		try{
			// step 1: 先調用 ISvcWordV2 的轉換函數（按需求固定順序）。
			var JnWord = await SvcWordV2.LlmDictWordToJnWordWithLearn(
				FrontendUserCtxMgr.GetDbUserCtx(),
				Req, Resp, Ct
			);
			// step 2: 轉換成功後再跳轉到編輯頁。
			GoToWordEditPage(JnWord);
		}catch(Exception Ex){
			// step 3: 捕獲特定語言映射異常，提供兩個選項。
			if(!IsNormLangMappingErr(Ex)){
				HandleErr(Ex);
			}
			var BtnGoCfg = new Button();
			BtnGoCfg.SetContent(I18n[K.GoToLanguageConfigPage]);
			BtnGoCfg.Click += (s,e)=>{
				_ = OpenNormLangMappingPage();
			};
			var BtnSkipCfg = new Button();
			BtnSkipCfg.SetContent(I18n[K.SkipConfigAndGoEditPage]);
			BtnSkipCfg.Click += (s,e)=>{
				_ = GoToWordEditPage(BuildFallbackJnWord(Req, Resp));
			};
			ShowDialog(
				I18n[K.LanguageMappingNotConfiguredChooseNext],
				[
					BtnGoCfg,
					BtnSkipCfg,
				]
			);
			return NIL;
		}
		return NIL;
	}

	public async partial Task<bool> QuickSaveToWord(CT Ct){
		if(AnyNull(SvcWordV2, FrontendUserCtxMgr, LastReqLlmDict, LastRespLlmDict)){
			ShowToast(I18n[K.CompleteDictionaryQueryBeforeSave]);
			return false;
		}
		var Req = LastReqLlmDict!;
		var Resp = LastRespLlmDict!;
		try{
			var JnWord = await SvcWordV2.LlmDictWordToJnWordWithLearn(
				FrontendUserCtxMgr.GetDbUserCtx(),
				Req, Resp, Ct
			);
			await SvcWordV2.MergeWord(
				FrontendUserCtxMgr.GetDbUserCtx(),
				ToolAsyE.ToAsyE([JnWord]),
				Ct
			);
			HasQuickSavedCurrentLookup = true;
			ShowToast(I18n[K.Saved]);
			return true;
		}catch(Exception Ex){
			HandleErr(Ex);
			return false;
		}
	}

	private partial bool IsNormLangMappingErr(Exception Ex){
		if(Ex is IAppErr AppErr){
			return ReferenceEquals(AppErr.Type, KeysErr.Word.NormLangToUserLangIsNotMapped);
		}
		return false;
	}

	private partial bool IsLlmResponseParseFailed(Exception Ex){
		if(Ex is IAppErr AppErr){
			return ReferenceEquals(AppErr.Type, KeysErr.Dictionary.LlmResponseParseFailed);
		}
		return false;
	}

	private static partial bool IsLookupCanceled(Exception Ex, CT Ct){
		if(Ct.IsCancellationRequested && Ex is OperationCanceledException){
			return true;
		}
		if(Ex.InnerException is not null){
			return IsLookupCanceled(Ex.InnerException, Ct);
		}
		return false;
	}

	private partial obj? OpenNormLangMappingPage(){
		// 直接進入新增映射頁，並預填當前源語言，減少用戶操作步驟。
		var View = new ViewNormLangToUserLangEdit();
		if(View.Ctx is not null){
			View.Ctx.SetCreateMode(true);
			View.Ctx.FromPoNormLangToUserLang(null);
			View.Ctx.PoNormLang = SrcLang;
		}
		ViewNavi?.GoTo(ToolView.WithTitle(I18n[K.AddNormLangToUserLang], View));
		return NIL;
	}

	private partial obj? GoToWordEditPage(JnWord JnWord){
		if(OnOpenWordEdit is null){
			ShowDialog(I18n[K.WordEditorOpenerNotReady]);
			return NIL;
		}
		OnOpenWordEdit(JnWord);
		return NIL;
	}

	private partial JnWord BuildFallbackJnWord(IReqLlmDict Req, IRespLlmDict Resp){
		var Head = !str.IsNullOrWhiteSpace(Resp.Head)
			? Resp.Head
			: Req.Query.Term
		;
		var Lang = Req.OptLang.SrcLang.Code ?? "";
		var Props = new List<PoWordProp>();

		// 把詞典描述映射成 description 屬性，後續可在編輯頁繼續調整。
		foreach(var Descr in Resp.Descrs){
			if(str.IsNullOrWhiteSpace(Descr)){
				continue;
			}
			Props.Add(new PoWordProp{
				KType = EKvType.Str,
				KStr = KeysProp.Inst.description,
				VType = EKvType.Str,
				VStr = Descr,
			});
		}

		// 把讀音也保留爲 pronunciation 屬性，避免查詢信息丟失。
		foreach(var Pron in Resp.Pronunciations){
			var Value = str.IsNullOrWhiteSpace(Pron.TextType)
				? Pron.Text
				: $"{Pron.TextType}: {Pron.Text}"
			;
			if(str.IsNullOrWhiteSpace(Value)){
				continue;
			}
			Props.Add(new PoWordProp{
				KType = EKvType.Str,
				KStr = KeysProp.Inst.pronunciation,
				VType = EKvType.Str,
				VStr = Value,
			});
		}

		var R = new JnWord{
			Word = new PoWord{
				Head = Head ?? "",
				Lang = Lang,
			},
			Props = Props,
			Learns = [],
		};
		R.EnsureForeignId();
		return R;
	}

	private async partial Task<nil> PersistCurLangs(CT Ct){
		if(AnyNull(SvcDictionary, FrontendUserCtxMgr)){
			return NIL;
		}
		try{
			var dbCtx = FrontendUserCtxMgr.GetDbUserCtx();
			var src = new PoNormLang{
				Type = ELangIdentType.Bcp47,
				Code = SrcLang,
			};
			var tgt = new PoNormLang{
				Type = ELangIdentType.Bcp47,
				Code = TgtLang,
			};
			await SvcDictionary.SetCurSrcNormLang(dbCtx, src, Ct);
			await SvcDictionary.SetCurTgtNormLang(dbCtx, tgt, Ct);
		}catch(Exception ex){
			HandleErr(ex);
		}
		return NIL;
	}

	private async partial Task<nil> PersistSrcLang(PoNormLang Po, CT Ct){
		if(AnyNull(SvcDictionary, FrontendUserCtxMgr)){
			return NIL;
		}
		try{
			await SvcDictionary.SetCurSrcNormLang(FrontendUserCtxMgr.GetDbUserCtx(), Po, Ct);
		}catch(Exception ex){
			HandleErr(ex);
		}
		return NIL;
	}

	private async partial Task<nil> PersistTgtLang(PoNormLang Po, CT Ct){
		if(AnyNull(SvcDictionary, FrontendUserCtxMgr)){
			return NIL;
		}
		try{
			await SvcDictionary.SetCurTgtNormLang(FrontendUserCtxMgr.GetDbUserCtx(), Po, Ct);
		}catch(Exception ex){
			HandleErr(ex);
		}
		return NIL;
	}

	private async partial Task<nil> RefreshLangTranslatedNames(CT Ct){
		if(AnyNull(SvcNormLang, FrontendUserCtxMgr)){
			UpdateLangDisplayTexts();
			return NIL;
		}
		try{
			var UiLang = KeysClientCfg.Lang.GetFrom(AppCfg.Inst) ?? "en";
			var TargetLang = new NormLang{
				Type = ELangIdentType.Bcp47,
				Code = UiLang,
			};
			List<str?> Names = [];
			var NameAsyE = SvcNormLang.OrdGetTranslatedName(
				FrontendUserCtxMgr.GetDbUserCtx(),
				TargetLang,
				ToolAsyE.ToAsyE([
					(INormLang)new NormLang{Type = ELangIdentType.Bcp47, Code = SrcLang},
					(INormLang)new NormLang{Type = ELangIdentType.Bcp47, Code = TgtLang},
				]),
				Ct
			);
			await foreach(var Name in NameAsyE.WithCancellation(Ct)){
				Names.Add(Name);
			}
			SrcLangTranslatedName = Names.Count > 0 ? (Names[0] ?? "") : "";
			TgtLangTranslatedName = Names.Count > 1 ? (Names[1] ?? "") : "";
		}catch(Exception ex){
			HandleErr(ex);
		}
		return NIL;
	}

	private partial nil RefreshSrcLangTagSelection(){
		foreach(var Tag in SrcLangTags){
			Tag.IsSelected = Tag.IsLang(ELangIdentType.Bcp47, SrcLang);
		}
		return NIL;
	}

	private partial VmNormLangTag LoadSrcLangTag(
		CfgDictionarySrcLangTag Cfg,
		PoNormLang? Po
	){
		var R = VmNormLangTag.Mk();
		R.FromCfg(Cfg, Po);
		R.IsSelected = R.IsLang(ELangIdentType.Bcp47, SrcLang);
		return R;
	}

	/// 建立大小寫不敏感的語言身份鍵，供批量查詢結果回填配置順序。
	private static partial str SrcLangTagKey(ELangIdentType Type, str? Code){
		return $"{(i32)Type}:{(Code ?? "").Trim().ToUpperInvariant()}";
	}

	private partial nil UpdateLangDisplayTexts(){
		SrcLangDisplay = FormatLangDisplay(SrcLang, SrcLangTranslatedName);
		TgtLangDisplay = FormatLangDisplay(TgtLang, TgtLangTranslatedName);
		return NIL;
	}

	private static partial str FormatLangDisplay(str Code, str? TranslatedName){
		if(str.IsNullOrWhiteSpace(TranslatedName)){
			return Code;
		}
		return $"{Code} {TranslatedName}";
	}

	private static partial str DescribeTextTail(str Name, str? Text){
		var Safe = Text ?? "";
		var TailLen = Math.Min(80, Safe.Length);
		var Tail = Safe[^TailLen..]
			.Replace("\r", "\\r")
			.Replace("\n", "\\n");
		return $"{Name}.Length={Safe.Length}; {Name}.Tail={Tail}";
	}

	private partial nil ResetQuickSaveState(){
		CanQuickSaveCurrentLookup = false;
		HasQuickSavedCurrentLookup = false;
		return NIL;
	}
}
