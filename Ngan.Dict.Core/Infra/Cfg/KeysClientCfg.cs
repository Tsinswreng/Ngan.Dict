namespace Ngaq.Core.Infra.Cfg;

using System.Collections;
using Ngaq.Core.Frontend.Hotkey;
using Ngaq.Core.Frontend.Kv;
using Tsinswreng.CsCfg;
using static Tsinswreng.CsCfg.CfgNode<obj?>;
//TODO 異常處理 勿緣用戶配置ʹ謬而致整程序崩

public class KeysClientCfg{
	public static ICfgNode<str> Lang = Mk(null, [nameof(Lang)], "en");

	public static ICfgNode<str> SqlitePath = Mk(null, [nameof(SqlitePath)], "./Ngaq.sqlite");

	public static ICfgNode<str> RwCfgPath = Mk(null, [nameof(RwCfgPath)], "Ngaq.Rw.jsonc");

	public class Background{
		public static ICfgNode _R = Mk(null, [nameof(Background)]);
		public static ICfgNode<str> Mode = Mk(_R, [nameof(Mode)], "");
		public static ICfgNode<IList<object?>> GalleryDirs = Mk(_R, [nameof(GalleryDirs)], (IList<object?>)[]);
	}
	public static ICfgNode<str> ServerBaseUrl = Mk(null, [nameof(ServerBaseUrl)], "");
	//public static ICfgNode Ui = Mk(null, [nameof(Ui)], null);
	public static class Ui{
		public static ICfgNode _R = Mk(null, [nameof(Ui)]);
		public static ICfgNode<f64> BaseFontSize = Mk(_R, [nameof(BaseFontSize)], 16.0);
	}
		

	public static class User{
		public static ICfgNode _R = Mk(null, [nameof(User)]);
		public static ICfgNode<str> CurrentUserId = Mk(_R, [nameof(CurrentUserId)], "0");
	}

	public class Word{
		public static ICfgNode _R = Mk(null, [nameof(Word)]);
		[Obsolete(@$"宜置于{nameof(KeysClientKv)}中")]
		public static ICfgNode<str> WordsPackExportPath = Mk(_R, [nameof(WordsPackExportPath)], "");
		[Obsolete(@$"宜置于{nameof(KeysClientKv)}中")]
		public static ICfgNode<str> WordsPackImportPath = Mk(_R, [nameof(WordsPackImportPath)], "");
		public static ICfgNode<u64> MaxDisplayedWordCount = Mk(_R, [nameof(MaxDisplayedWordCount)], 500ul);
		[Obsolete]
		public static ICfgNode<IList<obj?>?> FilterLanguage = Mk(_R, [nameof(FilterLanguage)], (IList<obj?>?)null);
		public static ICfgNode<bool> EnableRandomBackground = Mk(_R, [nameof(EnableRandomBackground)], false);
		[Doc("背單詞旹 點擊單詞後是否自動發音")]
		public static ICfgNode<bool> EnableAutoPronounce = Mk(_R, [nameof(EnableAutoPronounce)], false);
	}

	public class LlmDictionary{
		public static ICfgNode _R = Mk(null, [nameof(LlmDictionary)]);
		public static ICfgNode<str> ApiUrl = Mk(_R, [nameof(ApiUrl)], "");
		public static ICfgNode<str> ApiKey = Mk(_R, [nameof(ApiKey)], "");
		public static ICfgNode<str> Model = Mk(_R, [nameof(Model)], "");
		public static ICfgNode<str> Prompt = Mk(_R, [nameof(Prompt)], "");
		/// 透傳到 LLM 請求體頂層的額外 JSON 對象。
		/// 用於承載 provider-specific 參數；空字符串表示不附加。
		public static ICfgNode<str> ExtraBodyJson = Mk(_R, [nameof(ExtraBodyJson)], "");
	}

	/// 詞典頁的本地界面偏好。
	public class Dictionary{
		public static ICfgNode _R = Mk(null, [nameof(Dictionary)]);
		/// 源語言快捷標籤配置。
		/// 配置文件保存為對象陣列；陣列順序即快捷欄顯示順序。
		public static ICfgNode<IList<obj?>> SrcLangTags = Mk(
			_R,
			[nameof(SrcLangTags)],
			(IList<obj?>)[]
		);
	}

	/// 快捷鍵配置。
	public class Hotkey{
		public static ICfgNode _R = Mk(null, [nameof(Hotkey)]);

		/// 查詞快捷鍵：觸發後讀剪貼板並打開字典查詢。
		public class DictionaryLookup{
			public static ICfgNode _R = Mk(Hotkey._R, [nameof(DictionaryLookup)]);
			/// 修飾鍵，支持多值，使用 "|" 分隔，如 Ctrl|Shift。
			public static ICfgNode<str> Modifiers = Mk(_R, [nameof(Modifiers)], nameof(EHotkeyModifiers.Alt));
			/// 主鍵，值為 EHotkeyKey 枚舉名稱，如 E/F1/Enter。
			public static ICfgNode<str> Key = Mk(_R, [nameof(Key)], nameof(EHotkeyKey.E));
		}
	}
}

