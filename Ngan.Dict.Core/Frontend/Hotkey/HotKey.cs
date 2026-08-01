namespace Ngaq.Core.Frontend.Hotkey;

/// IHotKey 默认实现
public class HotKey : IHotKey {
	public str Id { get; set; } = "";
	public EHotkeyModifiers Modifiers { get; set; }
	public EHotkeyKey Key { get; set; }
	public FnOnHotKey OnHotkey { get; set; } = async(a,b)=>null;
}
