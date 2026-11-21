namespace Ngan.Dict.Ui.Views.Dictionary.SimpleWord;

using System.Collections.ObjectModel;
using Ngan.Dict.Core.Frontend.Clipboard;
using Ngan.Dict.Core.Frontend.User;
using Ngan.Dict.Core.Shared.Audio;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.Dictionary.Svc;
using Ngan.Dict.Core.Shared.Word.Models.DictionaryApi;
using Ngan.Dict.Ui.Infra;
using Ctx = VmSimpleWord;

/// 保存簡明查詞結果，並協調流式文本狀態與詞頭讀音播放。
public partial class VmSimpleWord: AppVmBase, IMk<Ctx>{
	/// 供靜態工廠建立無依賴實例；依賴注入使用唯一的公開構造器。
	protected partial VmSimpleWord();
	/// 建立不注入服務的簡明查詞結果 ViewModel。
	public static partial Ctx Mk();

	/// 設計期與調試期使用的示例集合。
	public static ObservableCollection<Ctx> Samples = [];

	/// 初始化調試示例；靜態構造器無法使用 partial，故保留在聲明文件。
	static VmSimpleWord(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
			o.Head = "word";
			o.Pronunciations = [Pronunciation.Sample.Samples[0]];
			o.Description =
"""
n.	詞；單詞；字；消息
v.	措辭；用詞
int.	說得對
网絡	話；一個字；文字處理
""";
		}
		#endif
	}

	/// TTS 服務：把詞頭轉成可播放音頻。
	ISvcTts? SvcTts;
	/// 音頻播放器：真正執行播放。
	IAudioPlayer? AudioPlayer;
	/// 字典服務：用於取得當前源語言配置。
	ISvcDictionary? SvcDictionary;
	/// 前端用戶上下文：取得字典服務所需的數據庫上下文。
	IFrontendUserCtxMgr? FrontendUserCtxMgr;
	/// 跨平臺剪貼板服務：將本次查詞的釋義文本交由當前平臺寫入系統剪貼板。
	ISvcClipboard? SvcClipboard;

	/// 注入讀音播放所需的服務。
	public partial VmSimpleWord(
		ISvcTts? SvcTts,
		IAudioPlayer? AudioPlayer,
		ISvcDictionary? SvcDictionary,
		IFrontendUserCtxMgr? FrontendUserCtxMgr,
		ISvcClipboard? SvcClipboard
	);

	/// 用最終結構化響應更新詞頭、讀音與釋義，並關閉流式寫入窗口。
	public partial nil FromRespLlmDict(IRespLlmDict Resp);
	/// 開始新的流式查詢並重置本輪展示狀態。
	/// 把結構化義項攤平成純文本，供複製／保存使用。
	public static partial str FormatSensesAsText(IList<DictSense> Senses);

	public partial nil StartStreaming(str QueryTerm);
	/// 接收尚未最終定稿的流式響應片段。
	public partial nil GotNewSeg(DtoOnNewSeg NewSeg);
	/// 主動結束流式寫入窗口，同時保留已展示文本。
	public partial nil StopStreaming();
	/// 按當前源語言合成並播放詞頭讀音。
	public partial Task<nil> PlayHead(CT Ct);
	/// 將當前結果的全部 Descr 依原有換行格式拼接後寫入系統剪貼板。
	/// 結果尚未產生可複製文本或當前平臺沒有剪貼板服務時，不進行寫入。
	public partial Task<nil> CopyDescription(CT Ct);

	/// 當前查詞結果的詞頭。
	public str Head{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 當前查詞結果的讀音列表。
	public IList<Pronunciation> Pronunciations{
		get;
		set{SetProperty(ref field, value);}
	} = [];

	/// 結構化義項：詞性／釋義／例句。
	/// 界面按它分塊排版；`Description` 仍保留為純文本，供複製與保存到詞庫使用。
	public IList<DictSense> Senses{
		get;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(HasSenses));
			}
		}
	} = [];

	/// 是否有結構化義項可顯示。
	public bool HasSenses => Senses.Count > 0;

	/// 當前查詞結果的合併釋義文本。翻譯模式下它等於 `Translation`，供「複製」沿用同一條路徑。
	public str Description{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 本輪結果的類型（詞／句／段）。翻譯模式由模型回包的 `Kind` 或請求端的判定給出。
	public EQueryKind Kind{
		get;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(IsTranslate));
				OnPropertyChanged(nameof(IsDict));
			}
		}
	} = EQueryKind.Word;

	/// 翻譯模式的原文（保持原樣，不經正規化）。
	public str SourceText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 翻譯模式的整段譯文。
	public str Translation{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 逐句對照；詞典模式為空。
	public IList<TranslationSegment> Segments{
		get;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(ShowSegments));
				OnPropertyChanged(nameof(ShowSourceText));
			}
		}
	} = [];

	/// 是否顯示逐句對照：只有一句時它與譯文重複，沒有展示價值。
	public bool ShowSegments => Segments.Count > 1;

	/// 是否顯示獨立的原文塊。
	/// *為何與逐句對照互斥*：逐句對照的每一行已經含原文，
	/// 兩者同時出現只是把同一段話印兩遍，反而看不清重點。
	public bool ShowSourceText => !ShowSegments;

	/// 術語／譯法說明；可為空。
	public IList<TranslationNote> Notes{
		get;
		set{SetProperty(ref field, value);}
	} = [];

	/// 是否為翻譯結果；界面據此切換詞典卡／翻譯卡。
	public bool IsTranslate => Kind != EQueryKind.Word;

	/// 是否為詞典結果（與 `IsTranslate` 互斥，供不需要轉換器的可見性綁定用）。
	public bool IsDict => Kind == EQueryKind.Word;

	/// 當前查詞是否已收到最終結果或被主動停止。
	/// 定稿後忽略 UI 線程中晚到的流式片段。
	bool IsFinalized{get;set;} = false;
}
