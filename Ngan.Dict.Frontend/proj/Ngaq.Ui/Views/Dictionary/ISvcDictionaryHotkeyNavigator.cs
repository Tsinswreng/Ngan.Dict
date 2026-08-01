namespace Ngaq.Ui.Views.Dictionary;

/// 字典頁導航服務（供熱鍵與其他入口復用）。
/// 約定：優先切換到 Home 內的字典底欄標簽。
public interface ISvcDictionaryHotkeyNavigator{
	/// 切到字典標簽。若提供詞條，則自動觸發查詢。
	/// <param name="Term">待查詞條；為空時僅打開頁面。</param>
	/// <returns>空值。</returns>
	public nil OpenDictionary(str? Term = null);
}
