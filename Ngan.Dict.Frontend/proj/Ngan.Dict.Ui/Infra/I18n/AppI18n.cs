namespace Ngan.Dict.Ui.Infra.I18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;

[Doc($$"""
本應用的 i18n 門面。

*本輪只落鍵、不落譯文*：鍵族在 {{nameof(KeysUiI18n)}}，已按 CsI18n 的 {{nameof(I18nKey)}} 聲明；
語言集合這邊先註冊一個空的語言包，故取值一律命中「查不到」而回鍵名——
界面會顯示 `View/Common/Confirm` 這樣的鍵，而不是崩掉。等譯文補上時，
只要往 {{nameof(AddLang)}} 傳入「鍵 → 模板」的表即可，調用點一行都不用改。

*為何要有 Set 而不用單一字典*：切換語言、可選語言清單、覆蓋率自檢都在 {{nameof(I18nSet)}}
裏；這一層只是把它固定成本應用的那一份。

#Eg[
	```cs
	// 取文案：鍵不存在時回鍵名（本輪的常態）
	var S = AppI18n.Inst.Get(KeysUiI18n.Common.Confirm);
	```
]
""")]
public static class AppI18n {

	[Doc("尚未提供任何譯文時使用的語言代碼；它同時是默認語言。")]
	public const str FallbackLang = "zh-Hant";

	[Doc("本應用的多語言集合；本輪只有一個空語言包。")]
	public static I18nSet Set { get; } = MkSet();

	[Doc("當前語言的門面；業務代碼取文案一律走它。")]
	public static II18n Inst => Set.Cur;

	[Doc($$"""
	補上一份譯文。譯文表的鍵是鍵節點的全路徑（見 `I18nDict.KeySep`）。

	#Prm[{{nameof(Lang)}}][語言代碼，例如 `zh-Hant`、`en`。]
	#Prm[{{nameof(Map)}}][「鍵路徑 → 模板」的映射；可為空。]
	""")]
	public static void AddLang(str Lang, IDictionary<str, str> Map) {
		Set.AddLang(Lang, Map);
	}

	// 建一個「只有空語言包」的集合，並切到默認語言。
	// 空包不是錯誤狀態：CsI18n 的字典在查不到時會走 OnKeyNotFound，默認回鍵名。
	static I18nSet MkSet() {
		var R = new I18nSet { DfltLang = FallbackLang };
		R.AddLang(FallbackLang, new Dictionary<str, str>());
		R.Use(FallbackLang);
		return R;
	}
}
