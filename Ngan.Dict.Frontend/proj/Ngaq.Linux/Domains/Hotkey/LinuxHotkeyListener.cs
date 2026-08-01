namespace Ngaq.Linux.Domains.Hotkey;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ngaq.Core.Frontend.Hotkey;
using Tsinswreng.CsErr;

/// Linux 平台全局熱鍵監聽器（Ubuntu 22: X11/XWayland）。
/// 使用 X11 的 XGrabKey 註冊全局熱鍵，並在後台線程輪詢按鍵事件。
public class LinuxHotkeyListener : IHotkeyListener{
	private readonly ILogger _logger;
	private readonly object _sync = new();

	/// 按業務 Id 存儲註冊信息。
	private readonly Dictionary<str, HotkeyRegistration> _registeredById = [];
	/// 按（keycode + modifiers）快速定位回調。
	private readonly Dictionary<KeyCombo, HotkeyRegistration> _registeredByCombo = [];

	private IntPtr _display = IntPtr.Zero;
	private IntPtr _rootWindow = IntPtr.Zero;
	private Thread? _eventThread;
	private bool _isRunning;

	private static int _x11ThreadInitState;

	private const int KeyPress = 2;
	private const int GrabModeAsync = 1;

	private const uint ShiftMask = 1u << 0;
	private const uint LockMask = 1u << 1;
	private const uint ControlMask = 1u << 2;
	private const uint Mod1Mask = 1u << 3; // Alt
	private const uint Mod2Mask = 1u << 4; // NumLock 常見映射
	private const uint Mod4Mask = 1u << 6; // Super/Win

	private const uint RelevantModifierMask = ShiftMask | ControlMask | Mod1Mask | Mod4Mask;

	private static readonly uint[] LockModifierVariants = [
		0u,
		LockMask,
		Mod2Mask,
		LockMask | Mod2Mask
	];

	private sealed class HotkeyRegistration{
		public required IHotKey HotKey{get; init;}
		public required uint KeyCode{get; init;}
		public required uint BaseModifiers{get; init;}
	}

	private readonly record struct KeyCombo(uint KeyCode, uint Modifiers);

	public LinuxHotkeyListener(ILogger Logger){
		_logger = Logger;
	}

	/// 註冊全局熱鍵。
	/// <param name="HotKey">熱鍵定義（包含 id、組合鍵與回調）。</param>
	/// <returns>成功返回 Ok，失敗返回錯誤。</returns>
	public IAnswer<obj?> Register(IHotKey HotKey){
		var Answer = new Answer<obj?>();
		lock(_sync){
			if(_registeredById.ContainsKey(HotKey.Id)){
				return Answer.AddErr($"Hotkey already registered: {HotKey.Id}");
			}

			if(!TryEnsureBackendReady(out var BackendError)){
				return Answer.AddErr(BackendError);
			}

			if(!TryConvertToX11KeyCode(HotKey.Key, out var KeyCode, out var KeyError)){
				return Answer.AddErr(KeyError);
			}

			var BaseModifiers = ConvertModifiers(HotKey.Modifiers);
			var Combo = new KeyCombo(KeyCode, BaseModifiers);
			if(_registeredByCombo.ContainsKey(Combo)){
				return Answer.AddErr($"Hotkey combination already registered: {HotKey.Modifiers}+{HotKey.Key}");
			}

			foreach(var ModifierVariant in LockModifierVariants){
				_ = XGrabKey(
					_display,
					(int)KeyCode,
					BaseModifiers | ModifierVariant,
					_rootWindow,
					0,
					GrabModeAsync,
					GrabModeAsync
				);
			}
			_ = XSync(_display, 0);

			var Registration = new HotkeyRegistration{
				HotKey = HotKey,
				KeyCode = KeyCode,
				BaseModifiers = BaseModifiers,
			};
			_registeredById[HotKey.Id] = Registration;
			_registeredByCombo[Combo] = Registration;
			return Answer.OkWith(NIL);
		}
	}

	/// 註銷全局熱鍵。
	/// <param name="HotkeyId">熱鍵 id。</param>
	/// <returns>成功返回 Ok，找不到則返回錯誤。</returns>
	public IAnswer<obj?> Unregister(str HotkeyId){
		var Answer = new Answer<obj?>();
		lock(_sync){
			if(!_registeredById.TryGetValue(HotkeyId, out var Registration)){
				return Answer.AddErr($"Hotkey not found: {HotkeyId}");
			}

			if(_display != IntPtr.Zero){
				foreach(var ModifierVariant in LockModifierVariants){
					_ = XUngrabKey(
						_display,
						(int)Registration.KeyCode,
						Registration.BaseModifiers | ModifierVariant,
						_rootWindow
					);
				}
				_ = XSync(_display, 0);
			}

			_registeredById.Remove(HotkeyId);
			_registeredByCombo.Remove(new KeyCombo(Registration.KeyCode, Registration.BaseModifiers));
			return Answer.OkWith(NIL);
		}
	}

	/// 清理全部已註冊熱鍵與 X11 連接。
	/// <returns>始終返回 Ok。</returns>
	public IAnswer<obj?> Cleanup(){
		Thread? EventThreadToJoin = null;
		lock(_sync){
			if(_display != IntPtr.Zero){
				foreach(var Registration in _registeredById.Values){
					foreach(var ModifierVariant in LockModifierVariants){
						_ = XUngrabKey(
							_display,
							(int)Registration.KeyCode,
							Registration.BaseModifiers | ModifierVariant,
							_rootWindow
						);
					}
				}
				_ = XSync(_display, 0);
			}

			_registeredById.Clear();
			_registeredByCombo.Clear();

			CloseBackendNoLock();
			EventThreadToJoin = _eventThread;
			_eventThread = null;
		}

		EventThreadToJoin?.Join(TimeSpan.FromMilliseconds(200));
		return new Answer<obj?>().OkWith(NIL);
	}

	/// 確保 X11 後端可用，並啓動事件循環線程。
	private bool TryEnsureBackendReady(out str ErrorMessage){
		ErrorMessage = "";
		if(_display != IntPtr.Zero){
			return true;
		}

		if(Interlocked.Exchange(ref _x11ThreadInitState, 1) == 0){
			_ = XInitThreads();
		}

		_display = XOpenDisplay(IntPtr.Zero);
		if(_display == IntPtr.Zero){
			var SessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE") ?? "";
			var DisplayVar = Environment.GetEnvironmentVariable("DISPLAY") ?? "";
			ErrorMessage = $"Failed to open X11 display. XDG_SESSION_TYPE={SessionType}, DISPLAY={DisplayVar}. Ubuntu 22 needs X11/XWayland for global hotkey.";
			return false;
		}

		_rootWindow = XDefaultRootWindow(_display);
		if(_rootWindow == IntPtr.Zero){
			ErrorMessage = "Failed to resolve X11 root window.";
			CloseBackendNoLock();
			return false;
		}

		_isRunning = true;
		_eventThread = new Thread(EventLoop){
			IsBackground = true,
			Name = nameof(LinuxHotkeyListener)
		};
		_eventThread.Start();
		return true;
	}

	/// 後台輪詢 X11 事件，命中熱鍵後調度回調。
	private void EventLoop(){
		while(true){
			List<IHotKey> PendingCallbacks = [];
			lock(_sync){
				if(!_isRunning || _display == IntPtr.Zero){
					return;
				}

				while(XPending(_display) > 0){
					_ = XNextEvent(_display, out var Event);
					if(Event.type != KeyPress){
						continue;
					}

					var PressedKeyCode = (uint)Event.xkey.keycode;
					var PressedModifiers = NormalizeModifiers(Event.xkey.state);
					if(_registeredByCombo.TryGetValue(new KeyCombo(PressedKeyCode, PressedModifiers), out var Registration)){
						PendingCallbacks.Add(Registration.HotKey);
					}
				}
			}

			foreach(var HotKey in PendingCallbacks){
				_ = Task.Run(async () => {
					try{
						await HotKey.OnHotkey(null, default);
					}catch(Exception Ex){
						_logger?.LogError(Ex, "Linux hotkey callback failed: {HotkeyId}", HotKey.Id);
					}
				});
			}

			Thread.Sleep(20);
		}
	}

	/// 關閉 X11 資源。調用方需持有 _sync 鎖。
	private void CloseBackendNoLock(){
		_isRunning = false;
		if(_display != IntPtr.Zero){
			_ = XCloseDisplay(_display);
		}
		_display = IntPtr.Zero;
		_rootWindow = IntPtr.Zero;
	}

	/// 將業務熱鍵枚舉轉換爲 X11 keycode。
	private bool TryConvertToX11KeyCode(EHotkeyKey Key, out uint KeyCode, out str ErrorMessage){
		KeyCode = 0;
		ErrorMessage = "";
		var KeySymName = KeyToX11KeysymName(Key);
		if(KeySymName == null){
			ErrorMessage = $"Unsupported hotkey key on Linux: {Key}";
			return false;
		}

		var KeySym = XStringToKeysym(KeySymName);
		if(KeySym == UIntPtr.Zero){
			ErrorMessage = $"X11 keysym not found for key: {Key} ({KeySymName})";
			return false;
		}

		KeyCode = XKeysymToKeycode(_display, KeySym);
		if(KeyCode == 0){
			ErrorMessage = $"X11 keycode not found for key: {Key} ({KeySymName})";
			return false;
		}

		return true;
	}

	/// 將修飾符枚舉映射到 X11 mask。
	private static uint ConvertModifiers(EHotkeyModifiers Modifiers){
		uint Code = 0;
		if((Modifiers & EHotkeyModifiers.Ctrl) != 0){
			Code |= ControlMask;
		}
		if((Modifiers & EHotkeyModifiers.Shift) != 0){
			Code |= ShiftMask;
		}
		if((Modifiers & EHotkeyModifiers.Alt) != 0){
			Code |= Mod1Mask;
		}
		if((Modifiers & EHotkeyModifiers.Win) != 0){
			Code |= Mod4Mask;
		}
		return Code;
	}

	/// 清理 Lock/NumLock 狀態位，避免影響快捷鍵匹配。
	private static uint NormalizeModifiers(uint RawModifiers){
		return RawModifiers & RelevantModifierMask;
	}

	/// 將業務熱鍵枚舉映射爲 X11 keysym 名稱。
	private static str? KeyToX11KeysymName(EHotkeyKey Key){
		return Key switch{
			EHotkeyKey.D0 => "0",
			EHotkeyKey.D1 => "1",
			EHotkeyKey.D2 => "2",
			EHotkeyKey.D3 => "3",
			EHotkeyKey.D4 => "4",
			EHotkeyKey.D5 => "5",
			EHotkeyKey.D6 => "6",
			EHotkeyKey.D7 => "7",
			EHotkeyKey.D8 => "8",
			EHotkeyKey.D9 => "9",
			EHotkeyKey.Enter => "Return",
			EHotkeyKey.Escape => "Escape",
			EHotkeyKey.Backspace => "BackSpace",
			EHotkeyKey.Tab => "Tab",
			EHotkeyKey.Space => "space",
			EHotkeyKey.Delete => "Delete",
			EHotkeyKey.Home => "Home",
			EHotkeyKey.End => "End",
			EHotkeyKey.PageUp => "Prior",
			EHotkeyKey.PageDown => "Next",
			EHotkeyKey.Up => "Up",
			EHotkeyKey.Down => "Down",
			EHotkeyKey.Left => "Left",
			EHotkeyKey.Right => "Right",
			EHotkeyKey.Print => "Print",
			EHotkeyKey.Pause => "Pause",
			EHotkeyKey.Insert => "Insert",
			EHotkeyKey.NumLock => "Num_Lock",
			EHotkeyKey.CapsLock => "Caps_Lock",
			EHotkeyKey.ScrollLock => "Scroll_Lock",
			_ => Key.ToString()
		};
	}

	[DllImport("libX11.so.6")]
	private static extern int XInitThreads();

	[DllImport("libX11.so.6")]
	private static extern IntPtr XOpenDisplay(IntPtr DisplayName);

	[DllImport("libX11.so.6")]
	private static extern int XCloseDisplay(IntPtr Display);

	[DllImport("libX11.so.6")]
	private static extern IntPtr XDefaultRootWindow(IntPtr Display);

	[DllImport("libX11.so.6")]
	private static extern UIntPtr XStringToKeysym(string String);

	[DllImport("libX11.so.6")]
	private static extern uint XKeysymToKeycode(IntPtr Display, UIntPtr KeySym);

	[DllImport("libX11.so.6")]
	private static extern int XGrabKey(
		IntPtr Display,
		int KeyCode,
		uint Modifiers,
		IntPtr GrabWindow,
		int OwnerEvents,
		int PointerMode,
		int KeyboardMode
	);

	[DllImport("libX11.so.6")]
	private static extern int XUngrabKey(IntPtr Display, int KeyCode, uint Modifiers, IntPtr GrabWindow);

	[DllImport("libX11.so.6")]
	private static extern int XPending(IntPtr Display);

	[DllImport("libX11.so.6")]
	private static extern int XNextEvent(IntPtr Display, out XEvent Event);

	[DllImport("libX11.so.6")]
	private static extern int XSync(IntPtr Display, int Discard);

	// XEvent 在 64-bit X11 上是 24 * sizeof(long) = 192 bytes。
	// 顯式指定 Size，避免 XNextEvent 寫入時超出託管結構尺寸導致崩潰。
	[StructLayout(LayoutKind.Explicit, Size = 192)]
	private struct XEvent{
		[FieldOffset(0)]
		public int type;
		[FieldOffset(0)]
		public XKeyEvent xkey;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct XKeyEvent{
		public int type;
		public ulong serial;
		public int send_event;
		public IntPtr display;
		public IntPtr window;
		public IntPtr root;
		public IntPtr subwindow;
		public ulong time;
		public int x;
		public int y;
		public int x_root;
		public int y_root;
		public uint state;
		public uint keycode;
		public int same_screen;
	}
}
