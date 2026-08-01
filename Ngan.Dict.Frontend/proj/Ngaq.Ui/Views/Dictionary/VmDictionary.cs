namespace Ngaq.Ui.Views.Dictionary;

using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
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
using Ngaq.Ui.Views.Dictionary.SimpleWord;
using Ngaq.Ui.Views.Dictionary.NormLangTag;
using Tsinswreng.CsTools;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

using Ctx = VmDictionary;

public partial class VmDictionary: ViewModelBase, IMk<Ctx>{

	protected VmDictionary(){}
	public static partial Ctx Mk();

	IFrontendUserCtxMgr FrontendUserCtxMgr = default!;
	ISvcDictionary SvcDictionary = default!;
	ISvcWordV2 SvcWordV2 = default!;
	ISvcNormLang SvcNormLang = default!;
	public VmDictionary(
		ISvcDictionary SvcDictionary
		,IFrontendUserCtxMgr FrontendUserCtxMgr
		,ISvcWordV2 SvcWordV2
		,ISvcNormLang SvcNormLang
	){
		this.SvcDictionary = SvcDictionary;
		this.FrontendUserCtxMgr = FrontendUserCtxMgr;
		this.SvcWordV2 = SvcWordV2;
		this.SvcNormLang = SvcNormLang;
		base.Init();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmDictionary(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	/// 最近一次字典查詢請求。轉換到詞庫單詞時必須一起傳入。
	public IReqLlmDict? LastReqLlmDict{get;set;}
	/// 最近一次字典查詢完整響應。供「保存到詞庫」按鈕使用。
	public IRespLlmDict? LastRespLlmDict{get;set;}
	/// 最近一次 LLM 流式原始輸出文本（用于排查和展示）。
	public str LastLlmRawOutput{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 由 View 注入的「打開單詞編輯頁」動作，避免 VM 直接依賴 View 類型。
	public Func<JnWord, nil>? OnOpenWordEdit{get;set;}

	public str Input{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 是否已執行過查詞。未查詞前用於顯示頁面用法提示。
	public bool HasLookupStarted{
		get;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(ShowUsageGuide));
				OnPropertyChanged(nameof(ShowLookupResult));
			}
		}
	} = false;

	/// 未查詞前顯示下方引導文案。
	public bool ShowUsageGuide => !HasLookupStarted;

	/// 查詞後顯示結果區，避免首屏空白。
	public bool ShowLookupResult => HasLookupStarted;

	/// 當前這一輪查詞是否已經完成到可快速保存的階段。
	/// 這是詞典頁面的業務狀態，不應放在 View 層保存。
	public bool CanQuickSaveCurrentLookup{
		get;
		set{SetProperty(ref field, value);}
	} = false;

	/// 當前查詞結果是否已經使用過快速保存。
	/// 以「本輪查詞結果」為粒度控制收藏按鈕只能成功一次。
	public bool HasQuickSavedCurrentLookup{
		get;
		set{SetProperty(ref field, value);}
	} = false;

	public VmSimpleWord? Result{
		get;
		set{SetProperty(ref field, value);}
	} = App.DiOrMk<VmSimpleWord>();

	public str SrcLang{
		get;
		set{
			if(SetProperty(ref field, value)){
				UpdateLangDisplayTexts();
				RefreshSrcLangTagSelection();
			}
		}
	} = "en";

	public str TgtLang{
		get;
		set{
			if(SetProperty(ref field, value)){
				UpdateLangDisplayTexts();
			}
		}
	} = "zh";

	/// 源語言的界面譯名，用於按鈕顯示。
	public str SrcLangTranslatedName{
		get;
		set{
			if(SetProperty(ref field, value)){
				UpdateLangDisplayTexts();
			}
		}
	} = "";

	/// 目標語言的界面譯名，用於按鈕顯示。
	public str TgtLangTranslatedName{
		get;
		set{
			if(SetProperty(ref field, value)){
				UpdateLangDisplayTexts();
			}
		}
	} = "";

	/// 源語言按鈕顯示文本，格式如 `en 英語`。
	public str SrcLangDisplay{
		get;
		set{SetProperty(ref field, value);}
	} = "en";

	/// 目標語言按鈕顯示文本，格式如 `zh 中文`。
	public str TgtLangDisplay{
		get;
		set{SetProperty(ref field, value);}
	} = "zh";

	/// 詞典頁源語言快捷標籤；集合順序與本地配置文件一致。
	/// 沒有配置時保持空集合，不自動加入任何語言。
	public ObservableCollection<VmNormLangTag> SrcLangTags{
		get;
		set{SetProperty(ref field, value);}
	} = [];

	public partial nil SwapLang();
	public partial nil ApplySrcNormLang(PoNormLang Po);
	public partial nil ApplyTgtNormLang(PoNormLang Po);
	public partial Task<nil> InitLang(CT Ct);
	public partial Task<nil> Lookup(CT Ct);
	/// 從本地配置讀取源語言快捷標籤，並補齊最新標準語言資料。
	public partial Task<nil> LoadSrcLangTags(CT Ct);
	/// 由快捷 Tag 直接切換源語言；目標語言保持不變。
	public partial nil ApplySrcNormLangTag(VmNormLangTag Tag);
	/// 用編輯頁保存後的結果整體替換快捷欄，並同步當前選中狀態。
	public partial nil ReplaceSrcLangTags(IEnumerable<VmNormLangTag> Tags);

	/// 流式片段很多時，若每段都單獨 Post 到 UI 線程，安卓上容易把點擊事件排在很後面。
	/// 這裡把待顯示片段先合併，保證 UI 線程同一時刻最多掛一個刷新任務。
	private partial void QueueFlushStreamedSegToUi(
		StringBuilder PendingUiSeg,
		object StreamLock,
		IntBox UiFlushQueued
	);

	/// 查詞取消不屬於錯誤。
	/// 若尚未收到任何流式文本，回滾到查詢前；若已收到部分內容，則保留當前已展示文本並停止後續更新。
	private partial nil HandleLookupCanceled(
		IReqLlmDict Req,
		LookupResultSnapshot PrevResult,
		IReqLlmDict? PrevReq,
		IRespLlmDict? PrevResp,
		str PrevRawOutput,
		str StreamedText
	);

	/// 用編輯後的原始文本重新解析詞典結果，不重調 LLM。
	public partial Task<nil> ReparseFromRawOutput(str RawOutput, CT Ct);

	/// 構造查詞請求中的語言信息。
	/// 優先從標準語言表中補齊 NativeName / EnglishName，避免把不完整的語言信息傳給後端。
	private partial Task<OptLang> BuildReqOptLang(IDbUserCtx DbUserCtx, CT Ct);

	/// 無需訪問後端時的兜底請求。
	/// 例如「編輯原始響應後重解析」場景，只保留當前界面上可直接取得的語言代碼。
	private partial ReqLlmDictEvt BuildFallbackReqLlmDict();

	/// 從標準語言實體映射出請求用 DTO。
	/// 若查不到資料，至少保留 BCP47 代碼，避免請求結構缺失。
	private static partial NormLangDetail ToNormLangDetail(str Code, PoNormLang? Po);

	/// 查詢前快照：失敗時恢復到此狀態。
	private partial LookupResultSnapshot CaptureResultSnapshot(VmSimpleWord? Cur);

	/// 查詢失敗回滾 UI 文本。
	private partial nil RestoreResultSnapshot(LookupResultSnapshot Snapshot);

	/// 將最近一次詞典查詢結果轉為詞庫詞條，然後跳到單詞編輯頁。
	/// 注意: 此函數本身不落庫；最終保存由 ViewWordEditV2 的 Save 按鈕執行。
	public partial Task<nil> ToWordEdit(CT Ct);

	/// 將最近一次詞典查詢結果直接合併保存到詞庫。
	/// 用於詞典頁的快速保存按鈕；成功後不再進入編輯頁。
	public partial Task<bool> QuickSaveToWord(CT Ct);

	/// 判斷是否爲「NormLangToUserLang 未映射」的指定業務異常。
	private partial bool IsNormLangMappingErr(Exception Ex);

	/// 判斷是否爲「LLM 響應解析失敗」業務異常。
	private partial bool IsLlmResponseParseFailed(Exception Ex);

	/// 取消查詞時不應彈「未知錯誤」。
	/// 這裡遞歸展開 InnerException，兼容 HttpClient / Task 包裝出的取消異常。
	private static partial bool IsLookupCanceled(Exception Ex, CT Ct);

	/// 打開語言映射配置頁。
	private partial obj? OpenNormLangMappingPage();

	/// 跳到 WordEditV2，由用戶在編輯頁手動點 Save 才真正保存。
	private partial obj? GoToWordEditPage(JnWord JnWord);

	/// 語言未映射時的兜底詞條構造：保留查詢頭詞、源語和描述/讀音，供用戶在編輯頁手動確認。
	private partial JnWord BuildFallbackJnWord(IReqLlmDict Req, IRespLlmDict Resp);

	private partial Task<nil> PersistCurLangs(CT Ct);
	private partial Task<nil> PersistSrcLang(PoNormLang Po, CT Ct);
	private partial Task<nil> PersistTgtLang(PoNormLang Po, CT Ct);

	/// 按當前 UI 語言批量獲取源/目標語言譯名，供語言切換按鈕顯示。
	private partial Task<nil> RefreshLangTranslatedNames(CT Ct);

	/// 按當前源語言更新所有快捷 Tag 的選中狀態。
	private partial nil RefreshSrcLangTagSelection();
	/// 將快捷標籤配置解析為運行時 ViewModel；找不到標準語言時仍保留 Code。
	private partial VmNormLangTag LoadSrcLangTag(
		Ngaq.Core.Shared.Dictionary.Models.CfgDictionarySrcLangTag Cfg,
		PoNormLang? Po
	);
	/// 建立大小寫不敏感的語言身份鍵，供批量查詢結果按配置順序回填。
	private static partial str SrcLangTagKey(ELangIdentType Type, str? Code);

	/// 更新語言按鈕最終文本；查不到譯名時僅顯示語言代碼。
	private partial nil UpdateLangDisplayTexts();

	/// 語言代碼與譯名拼成單行顯示，避免 View 層自己拼接字符串。
	private static partial str FormatLangDisplay(str Code, str? TranslatedName);

	/// 只記錄文本尾部的可見診斷信息，避免把整段原始輸出完整刷進日誌。
	private static partial str DescribeTextTail(str Name, str? Text);

	/// 開始新查詞前重置快速保存狀態。
	private partial nil ResetQuickSaveState();

	/// Lookup 失敗回滾用 DTO。
	sealed class LookupResultSnapshot{
		public str Head{get;set;} = "";
		public str Description{get;set;} = "";
		public IList<Pronunciation> Pronunciations{get;set;} = [];
	}

	/// 用可變字段包一層，避免 lambda 直接捕獲 ref 參數導致編譯錯誤。
	sealed class IntBox{
		public i32 Value;
	}
}
