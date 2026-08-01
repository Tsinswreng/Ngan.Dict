namespace Ngaq.Ui.Views.Dictionary;

using Ngaq.Core.Frontend.Hotkey;

/// <summary>
/// 熱鍵觸發的「剪貼板查詞」動作。
/// </summary>
public interface IHotkeyDictionaryLookupAction{
	/// <summary>
	/// 執行剪貼板查詞。
	/// </summary>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>熱鍵回調返回值。</returns>
	public Task<IRespHotKey?> Run(CT Ct);
}
