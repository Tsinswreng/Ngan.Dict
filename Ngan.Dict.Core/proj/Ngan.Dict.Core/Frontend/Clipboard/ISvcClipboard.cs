namespace Ngan.Dict.Core.Frontend.Clipboard;

public interface ISvcClipboard{
	[Doc(@$"
	#Rtn[null when clipboard content is not string]
	")]
	public Task<str?> GetText(CT Ct);

	/// 將指定純文本寫入系統剪貼板。
	/// <param name="Text">要複製的文本；由調用方決定換行與拼接格式。</param>
	/// <param name="Ct">取消令牌。</param>
	public Task<nil> SetText(str Text, CT Ct);
}
