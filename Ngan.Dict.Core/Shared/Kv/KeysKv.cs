using Ngaq.Core.Frontend.Kv;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngaq.Core.Tools;

namespace Ngaq.Core.Shared.Kv;

using static Ngaq.Core.Tools.SlashSepKey;
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
