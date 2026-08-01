namespace Ngaq.Core.Frontend.Hotkey;
using Tsinswreng.CsErr;


/// 全局快捷键管理接口
/// 提供跨平台注册、注销及回调功能
public interface IHotkeyListener{
	/// 注册全局快捷键
	/// <param name="HotKey">快捷键对象（包含标识、修饰符、主键和回调）</param>
	/// 成功則內返null
	public IAnswer<obj?> Register(IHotKey HotKey);

	/// 注销快捷键
	/// <param name="HotkeyId">快捷键唯一标识符</param>
	/// 成功則內返null
	public IAnswer<obj?> Unregister(str HotkeyId);

	/// 成功則內返null
	public IAnswer<obj?> Cleanup();
}



