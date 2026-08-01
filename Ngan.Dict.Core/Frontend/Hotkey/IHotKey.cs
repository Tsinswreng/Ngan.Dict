namespace Ngaq.Core.Frontend.Hotkey;

/// 快捷键模型定义接口
public interface IHotKey{
	/// 快捷键唯一标识
	str Id { get; set; }
	
	/// 修饰键组合 (Ctrl, Shift, Alt, Win)
	EHotkeyModifiers Modifiers { get; set; }
	
	/// 主键
	EHotkeyKey Key { get; set; }
	
	/// 按键触发时的回调
	FnOnHotKey OnHotkey { get; set; }
}
