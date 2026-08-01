namespace Ngaq.Ui.Views.Dictionary.NormLangTag;

using Ngaq.Core.Shared.Dictionary.Models;
using Tsinswreng.CsCfg;

/// 詞典源語言快捷標籤配置的讀寫工具。
/// 隔離配置庫的弱類型對象陣列，使 ViewModel 僅處理強類型配置 DTO。
public static partial class DictionarySrcLangTagCfg{
	/// 從本地配置讀取快捷標籤；無配置或格式無效時返回空集合。
	public static partial IList<CfgDictionarySrcLangTag> Load(ICfgAccessor Cfg);
	/// 將快捷標籤按當前順序寫入本地配置文件。
	public static partial Task<nil> Save(
		ICfgAccessor Cfg,
		IEnumerable<CfgDictionarySrcLangTag> Tags,
		CT Ct
	);
	/// 將配置庫返回的單個弱類型對象轉為強類型配置項。
	private static partial CfgDictionarySrcLangTag? ParseItem(obj? Raw);
	/// 將強類型配置項轉為配置文件可直接保存的對象。
	private static partial obj ToRaw(CfgDictionarySrcLangTag Tag);
}
