namespace Ngaq.Ui.Views.Dictionary.SimpleWord;

using System.Collections.ObjectModel;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Shared.Audio;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Svc;
using Ngaq.Core.Shared.Word.Models.DictionaryApi;
using Ngaq.Ui.Infra;
using Ctx = VmSimpleWord;

/// 保存簡明查詞結果，並協調流式文本狀態與詞頭讀音播放。
public partial class VmSimpleWord: ViewModelBase, IMk<Ctx>{
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

	/// 注入讀音播放所需的服務。
	public partial VmSimpleWord(
		ISvcTts? SvcTts,
		IAudioPlayer? AudioPlayer,
		ISvcDictionary? SvcDictionary,
		IFrontendUserCtxMgr? FrontendUserCtxMgr
	);

	/// 用最終結構化響應更新詞頭、讀音與釋義，並關閉流式寫入窗口。
	public partial nil FromRespLlmDict(IRespLlmDict Resp);
	/// 開始新的流式查詢並重置本輪展示狀態。
	public partial nil StartStreaming(str QueryTerm);
	/// 接收尚未最終定稿的流式響應片段。
	public partial nil GotNewSeg(DtoOnNewSeg NewSeg);
	/// 主動結束流式寫入窗口，同時保留已展示文本。
	public partial nil StopStreaming();
	/// 按當前源語言合成並播放詞頭讀音。
	public partial Task<nil> PlayHead(CT Ct);

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

	/// 當前查詞結果的合併釋義文本。
	public str Description{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 當前查詞是否已收到最終結果或被主動停止。
	/// 定稿後忽略 UI 線程中晚到的流式片段。
	bool IsFinalized{get;set;} = false;
}
