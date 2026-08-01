namespace Ngaq.Core.Frontend.Hotkey;
using Tsinswreng.CsErr;
[Doc(@$"註冊所有全局快捷鍵
依平臺 各自實現此接口
")]
public interface I_RegisterGlobalHotKeys{
	/// 成功則內返null
	public IAnswer<obj?> RegisterGlobalHotKeys();
}
