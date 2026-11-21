namespace Ngan.Dict.Core.Infra.Cfg;

using Ngan.Dict.Core.Frontend.Hotkey;
using Ngan.Dict.Core.Frontend.Kv;
using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;

[Doc($$"""
客戶端配置鍵族（CsKeyNode）。

*鍵與值分離*：鍵在此聲明（有靜態型別、有默認值、可跳轉），值由 {{nameof(AppCfg)}} 的兩個源按路徑取。
業務側一律寫 `AppCfg.Inst.Get(KeysClientCfg.Xxx)`，不得出現字符串鍵名。

*路徑是分段而非字符串*：取值收的是 `IReadOnlyList<str>`，故鍵上的分隔符只用於顯示與日誌。

*默認值的去處*：`Mk` 的第三個參數就是默認值。配置裡沒有這個鍵時，`ICfgReaderExtn.Get<T>` 直接回它，
業務側不必再寫一次 `?? 16.0`。
""")]
public class KeysClientCfg : KeysDeclBase<KeysClientCfg>, IStaticKeysDecl {

	[Doc("本族共用的節點製造器；分隔符只影響顯示，不參與取值。")]
	public static MkrKeyNode _MkrKeyNode { get; } = new() { PathSep = "/" };

	[Doc("界面語言代碼，如 `zh-Hant`、`en`。")]
	public static KeyNode<str> Lang = Mk(null, [nameof(Lang)], "en");

	[Doc("本機 sqlite 檔的路徑，相對應用儲存區。")]
	public static KeyNode<str> SqlitePath = Mk(null, [nameof(SqlitePath)], "./Ngan.Dict.sqlite");

	[Doc($$"""
	引導鍵：GUI 保存的那份配置檔的路徑，相對應用儲存區。

	它的默認值是默認檔名的*唯一*出處；出廠配置（只讀那份）可以改寫它，
	故實際位置要讀過出廠配置之後才決定（見 {{nameof(AppCfg)}}）。
	""")]
	public static KeyNode<str> RwCfgPath = Mk(null, [nameof(RwCfgPath)], "Ngan.Dict.Rw.jsonc");

	public class Background{
		public static KeyNode<obj?> _R = Mk(null, [nameof(Background)]);
		public static KeyNode<str> Mode = Mk(_R, [nameof(Mode)], "");
		public static KeyNode<IList<obj?>> GalleryDirs = Mk(_R, [nameof(GalleryDirs)], (IList<obj?>)[]);
	}

	[Doc("遠端服務基地址；空串表示不啟用遠端。")]
	public static KeyNode<str> ServerBaseUrl = Mk(null, [nameof(ServerBaseUrl)], "");

	public static class Ui{
		public static KeyNode<obj?> _R = Mk(null, [nameof(Ui)]);
		public static KeyNode<f64> BaseFontSize = Mk(_R, [nameof(BaseFontSize)], 16.0);
	}

	public static class User{
		public static KeyNode<obj?> _R = Mk(null, [nameof(User)]);
		public static KeyNode<str> CurrentUserId = Mk(_R, [nameof(CurrentUserId)], "0");
	}

	public class Word{
		public static KeyNode<obj?> _R = Mk(null, [nameof(Word)]);
		[Obsolete(@$"宜置于{nameof(KeysClientKv)}中")]
		public static KeyNode<str> WordsPackExportPath = Mk(_R, [nameof(WordsPackExportPath)], "");
		[Obsolete(@$"宜置于{nameof(KeysClientKv)}中")]
		public static KeyNode<str> WordsPackImportPath = Mk(_R, [nameof(WordsPackImportPath)], "");
		public static KeyNode<u64> MaxDisplayedWordCount = Mk(_R, [nameof(MaxDisplayedWordCount)], 500ul);
		[Obsolete]
		public static KeyNode<IList<obj?>?> FilterLanguage = Mk(_R, [nameof(FilterLanguage)], (IList<obj?>?)null);
		public static KeyNode<bool> EnableRandomBackground = Mk(_R, [nameof(EnableRandomBackground)], false);
		[Doc("背單詞旹 點擊單詞後是否自動發音")]
		public static KeyNode<bool> EnableAutoPronounce = Mk(_R, [nameof(EnableAutoPronounce)], false);
	}

	public class LlmDictionary{
		public static KeyNode<obj?> _R = Mk(null, [nameof(LlmDictionary)]);
		public static KeyNode<str> ApiUrl = Mk(_R, [nameof(ApiUrl)], "");
		public static KeyNode<str> ApiKey = Mk(_R, [nameof(ApiKey)], "");
		public static KeyNode<str> Model = Mk(_R, [nameof(Model)], "");
		public static KeyNode<str> Prompt = Mk(_R, [nameof(Prompt)], "");
		/// 翻譯（整句／整段）用的系統提示詞。
		/// 留空時用內置的 `DfltPromptTranslate`；與查詞提示詞分開，是因為兩者的輸出約束不同：
		/// 查詞要求詞頭正規化、讀音、按義項列釋義；翻譯要求原文一字不改、保留段落換行。
		public static KeyNode<str> PromptTranslate = Mk(_R, [nameof(PromptTranslate)], "");
		/// 透傳到 LLM 請求體頂層的額外 JSON 對象。
		/// 用於承載 provider-specific 參數；空字符串表示不附加。
		public static KeyNode<str> ExtraBodyJson = Mk(_R, [nameof(ExtraBodyJson)], "");
	}

	/// 詞典頁的本地界面偏好。
	public class Dictionary{
		public static KeyNode<obj?> _R = Mk(null, [nameof(Dictionary)]);
		/// 源語言快捷標籤配置。
		/// 配置文件保存為對象陣列；陣列順序即快捷欄顯示順序。
		public static KeyNode<IList<obj?>> SrcLangTags = Mk(
			_R,
			[nameof(SrcLangTags)],
			(IList<obj?>)[]
		);
	}

	/// 快捷鍵配置。
	public class Hotkey{
		public static KeyNode<obj?> _R = Mk(null, [nameof(Hotkey)]);

		/// 查詞快捷鍵：觸發後讀剪貼板並打開字典查詢。
		public class DictionaryLookup{
			public static KeyNode<obj?> _R = Mk(Hotkey._R, [nameof(DictionaryLookup)]);
			/// 修飾鍵，支持多值，使用 "|" 分隔，如 Ctrl|Shift。
			public static KeyNode<str> Modifiers = Mk(_R, [nameof(Modifiers)], nameof(EHotkeyModifiers.Alt));
			/// 主鍵，值為 EHotkeyKey 枚舉名稱，如 E/F1/Enter。
			public static KeyNode<str> Key = Mk(_R, [nameof(Key)], nameof(EHotkeyKey.E));
		}
	}
}
