namespace Ngaq.Ui.Views.Dictionary;

using Ngaq.Core.Frontend.Clipboard;
using Ngaq.Core.Frontend.Hotkey;

/// 熱鍵查詞動作實現：
/// 讀剪貼板 -> 切到 Home 的字典標簽 -> 帶詞查詢。
public class HotkeyDictionaryLookupAction : IHotkeyDictionaryLookupAction{
	readonly ISvcClipboard SvcClipboard;
	readonly ISvcDictionaryHotkeyNavigator DictionaryNavigator;

	public HotkeyDictionaryLookupAction(
		ISvcClipboard SvcClipboard,
		ISvcDictionaryHotkeyNavigator DictionaryNavigator
	){
		this.SvcClipboard = SvcClipboard;
		this.DictionaryNavigator = DictionaryNavigator;
	}

	/// 執行熱鍵查詞。
	/// <param name="Ct">取消令牌。</param>
	/// <returns>空回執。</returns>
	public async Task<IRespHotKey?> Run(CT Ct){
		var clipText = await SvcClipboard.GetText(Ct);
		DictionaryNavigator.OpenDictionary(clipText);
		return null;
	}
}
