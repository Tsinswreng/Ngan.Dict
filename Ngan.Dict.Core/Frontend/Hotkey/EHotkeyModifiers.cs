namespace Ngaq.Core.Frontend.Hotkey;
/// 快捷键修饰符枚举
[Flags]
public enum EHotkeyModifiers{
	None = 0,
	Ctrl = 1,
	Shift = 2,
	Alt = 4,
	Win = 8
}
