using Ngan.Dict.Core.Frontend.Kv;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngan.Dict.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngan.Dict.Core.Tools;

namespace Ngan.Dict.Core.Shared.Kv;

using static Ngan.Dict.Core.Tools.SlashSepKey;
using K = SlashSepKey;

[Doc(@$"鍵值。
如所存物 僅客戶端相關 應用 {nameof(KeysClientKv)}
")]
public partial class KeysKv{
	public class Dictionary{
		public static K _R = Mk(null, [nameof(Dictionary)]);
		// [Doc(@$"
		// 大模型字典 用戶最近使用過的正規化語言。
		// 格式爲 {nameof(IList<NormLang>)} json。
		// 臨時存、最多只存16個、避免性能問題。
		// ")]
		//[Obsolete]
		//public static K RecentUsedNormLangs = Mk(_R, [nameof(RecentUsedNormLangs)]);
		
		[Doc(@$"當前源語言。值格式爲{nameof(PoNormLang)}之json")]
		public static K CurSrcLang = Mk(_R, [nameof(CurSrcLang)]);
		[Doc(@$"當前目標語言。值格式爲{nameof(PoNormLang)}之json")]
		public static K CurTgtLang = Mk(_R, [nameof(CurTgtLang)]);
		// [Doc(@$"詞典系統提示語。值格式爲字符串。")]
		// public static K LlmDictSysPrompt = Mk(_R, [nameof(LlmDictSysPrompt)]);
	}
}
