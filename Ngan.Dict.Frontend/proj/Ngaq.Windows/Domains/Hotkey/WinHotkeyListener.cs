namespace Ngaq.Windows.Domains.Hotkey;
using Tsinswreng.CsErr;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Ngaq.Core.Frontend.Hotkey;
using Microsoft.Extensions.Logging;

/// Windows 平台的全局快捷键监听器实现
/// 使用 Win32 API RegisterHotKey 实现全局快捷键
public class WinHotkeyListener : IHotkeyListener{
	private ILogger Logger;
	private Dictionary<str, HotkeyRegistration> RegisteredHotkeys = [];
	// additional map for quick lookup by numeric id
	private Dictionary<int, HotkeyRegistration> _idMap = [];
	private int NextId = 1;

	/// 專用消息線程：既處理 Register/Unregister 工作，也接收 WM_HOTKEY。
	private Thread? _msgThread;
	private const int WM_HOTKEY = 0x0312;
	private const uint PM_REMOVE = 0x0001;
	/// 註冊/反註冊必須落在消息線程，所以用工作隊列跨線程派發。
	private readonly System.Collections.Concurrent.BlockingCollection<Action> _workQueue = new System.Collections.Concurrent.BlockingCollection<Action>();
	/// 確保消息線程已創建消息隊列，避免主線程把工作投遞後無人喚醒。
	private readonly ManualResetEventSlim _messageThreadReady = new(false);

	private class HotkeyRegistration{
		public int Id { get; set; }
		public IHotKey HotKey { get; set; } = null!;
	}

	public WinHotkeyListener(ILogger Logger){
		this.Logger = Logger;
	}

	#region Win32 API

	[DllImport("user32.dll")]
	private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

	[DllImport("user32.dll")]
	private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

	private const uint MOD_CTRL = 2;
	private const uint MOD_SHIFT = 4;
	private const uint MOD_ALT = 1;
	private const uint MOD_WIN = 8;

	#endregion

	public IAnswer<obj?> Register(IHotKey HotKey){
		var answer = new Answer<obj?>();
		// ensure the message thread is running so work queue will be handled
		EnsureMessageThreadRunning();
		_messageThreadReady.Wait();
		var tcs = new TaskCompletionSource<bool>();
		_workQueue.Add(() => {
			bool success = false;
			try{
				if(RegisteredHotkeys.ContainsKey(HotKey.Id)){
					Logger?.LogWarning("Hotkey {HotkeyId} already registered", HotKey.Id);
					success = false;
				} else {
					int Id = NextId++;
					uint ModifiersCode = ConvertModifiers(HotKey.Modifiers);
					uint KeyCode = ConvertKey(HotKey.Key);
					// register on the message thread
					success = RegisterHotKey(IntPtr.Zero, Id, ModifiersCode, KeyCode);
					if(success){
						var Registration = new HotkeyRegistration{
							Id = Id,
							HotKey = HotKey
						};
						RegisteredHotkeys[HotKey.Id] = Registration;
						_idMap[Id] = Registration;
						Logger?.LogInformation("Hotkey {HotkeyId} registered successfully with id {Id}", HotKey.Id, Id);
					} else {
						Logger?.LogError("Failed to register hotkey {HotkeyId}", HotKey.Id);
					}
				}
			}catch(Exception ex){
				Logger?.LogError(ex, "Exception during hotkey registration {HotkeyId}", HotKey.Id);
				success = false;
			}
			tcs.SetResult(success);
		});
		bool ok = tcs.Task.Result;
		if(!ok)answer.AddErr("registration failed");
		else answer.Ok = true;
		return answer;
	}

	public IAnswer<obj?> Unregister(str HotkeyId){
		var ans = new Answer<obj?>();
		if(!RegisteredHotkeys.TryGetValue(HotkeyId, out var Registration)){
			Logger?.LogWarning("Hotkey {HotkeyId} not found", HotkeyId);
			ans.AddErr("not found");
			return ans;
		}
		// perform on message thread to match registration
		EnsureMessageThreadRunning();
		_messageThreadReady.Wait();
		var tcs = new TaskCompletionSource<bool>();
		_workQueue.Add(()=>{
			bool success = false;
			try{
				success = UnregisterHotKey(IntPtr.Zero, Registration.Id);
				if(success){
					RegisteredHotkeys.Remove(HotkeyId);
					_idMap.Remove(Registration.Id);
					Logger?.LogInformation("Hotkey {HotkeyId} unregistered successfully", HotkeyId);
				} else {
					Logger?.LogError("Failed to unregister hotkey {HotkeyId}", HotkeyId);
				}
			}catch(Exception ex){
				Logger?.LogError(ex, "Exception during hotkey unregistration {HotkeyId}", HotkeyId);
				success=false;
			}
			tcs.SetResult(success);
		});
		bool ok = tcs.Task.Result;
		if(!ok) ans.AddErr("unregister failed");
		else ans.Ok = true;
		return ans;
	}

	public IAnswer<obj?> Cleanup(){
		var ans = new Answer<obj?>();
		var HotkeyIds = new List<str>(RegisteredHotkeys.Keys);
		foreach(var HotkeyId in HotkeyIds){
			var r = Unregister(HotkeyId);
			if(!r.Ok){
				ans.AddErr("failed cleanup");
			}
		}
		Logger?.LogInformation("All hotkeys cleaned up");
		if(ans.Ok==false) ans.Ok=true; // success even if no entries
		return ans;
	}

	private void EnsureMessageThreadRunning(){
		if(_msgThread != null && _msgThread.IsAlive) return;
		_messageThreadReady.Reset();
		_msgThread = new Thread(MessageLoop){IsBackground=true};
		_msgThread.Start();
	}

	private void MessageLoop(){
		try{
			// 先顯式創建本線程的 Win32 消息隊列，再允許外部投遞註冊工作。
			_ = PeekMessage(out _, IntPtr.Zero, 0, 0, 0);
			_messageThreadReady.Set();
			while(true){
				// 先盡量清空工作隊列，避免 Register/Unregister 卡住 UI 啟動流程。
				while(_workQueue.TryTake(out var work, TimeSpan.Zero)){
					try{ work(); }catch(Exception ex){ Logger?.LogError(ex, "hotkey work queue item failed"); }
				}

				// 再非阻塞提取所有待處理消息；避免 GetMessage 提前阻塞導致競態死鎖。
				while(PeekMessage(out var msg, IntPtr.Zero, 0, 0, PM_REMOVE)){
					if(msg.message == WM_HOTKEY){
						int id = (int)msg.wParam;
						if(_idMap.TryGetValue(id, out var reg)){
							// fire callback on threadpool
							_ = Task.Run(()=> reg.HotKey.OnHotkey(null, default));
						}
					}
					TranslateMessage(ref msg);
					DispatchMessage(ref msg);
				}

				Thread.Sleep(10);
			}
		}catch(Exception ex){
			Logger?.LogError(ex, "Hotkey message loop exception");
		}
	}

	[DllImport("user32.dll")]
	private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
	[DllImport("user32.dll")]
	private static extern bool TranslateMessage([In] ref MSG lpMsg);
	[DllImport("user32.dll")]
	private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

	[StructLayout(LayoutKind.Sequential)]
	private struct MSG{
		public IntPtr hwnd;
		public uint message;
		public UIntPtr wParam;
		public IntPtr lParam;
		public uint time;
		public POINT pt;
	}
	[StructLayout(LayoutKind.Sequential)]
	private struct POINT{public int x; public int y;}


	/// 将修饰符枚举转换为 Win32 API 代码
	private uint ConvertModifiers(EHotkeyModifiers Modifiers){
		uint Code = 0;
		if((Modifiers & EHotkeyModifiers.Ctrl) != 0) Code |= MOD_CTRL;
		if((Modifiers & EHotkeyModifiers.Shift) != 0) Code |= MOD_SHIFT;
		if((Modifiers & EHotkeyModifiers.Alt) != 0) Code |= MOD_ALT;
		if((Modifiers & EHotkeyModifiers.Win) != 0) Code |= MOD_WIN;
		return Code;
	}


	/// 将快捷键枚举转换为 Win32 虚拟键代码
	private uint ConvertKey(EHotkeyKey Key){
		return Key switch{
			// 字母键 A-Z: 0x41-0x5A
			EHotkeyKey.A => 0x41, EHotkeyKey.B => 0x42, EHotkeyKey.C => 0x43, EHotkeyKey.D => 0x44,
			EHotkeyKey.E => 0x45, EHotkeyKey.F => 0x46, EHotkeyKey.G => 0x47, EHotkeyKey.H => 0x48,
			EHotkeyKey.I => 0x49, EHotkeyKey.J => 0x4A, EHotkeyKey.K => 0x4B, EHotkeyKey.L => 0x4C,
			EHotkeyKey.M => 0x4D, EHotkeyKey.N => 0x4E, EHotkeyKey.O => 0x4F, EHotkeyKey.P => 0x50,
			EHotkeyKey.Q => 0x51, EHotkeyKey.R => 0x52, EHotkeyKey.S => 0x53, EHotkeyKey.T => 0x54,
			EHotkeyKey.U => 0x55, EHotkeyKey.V => 0x56, EHotkeyKey.W => 0x57, EHotkeyKey.X => 0x58,
			EHotkeyKey.Y => 0x59, EHotkeyKey.Z => 0x5A,

			// 数字键 0-9: 0x30-0x39
			EHotkeyKey.D0 => 0x30, EHotkeyKey.D1 => 0x31, EHotkeyKey.D2 => 0x32, EHotkeyKey.D3 => 0x33,
			EHotkeyKey.D4 => 0x34, EHotkeyKey.D5 => 0x35, EHotkeyKey.D6 => 0x36, EHotkeyKey.D7 => 0x37,
			EHotkeyKey.D8 => 0x38, EHotkeyKey.D9 => 0x39,

			// 功能键 F1-F12: 0x70-0x7B
			EHotkeyKey.F1 => 0x70, EHotkeyKey.F2 => 0x71, EHotkeyKey.F3 => 0x72, EHotkeyKey.F4 => 0x73,
			EHotkeyKey.F5 => 0x74, EHotkeyKey.F6 => 0x75, EHotkeyKey.F7 => 0x76, EHotkeyKey.F8 => 0x77,
			EHotkeyKey.F9 => 0x78, EHotkeyKey.F10 => 0x79, EHotkeyKey.F11 => 0x7A, EHotkeyKey.F12 => 0x7B,

			// 特殊键
			EHotkeyKey.Enter => 0x0D,
			EHotkeyKey.Escape => 0x1B,
			EHotkeyKey.Backspace => 0x08,
			EHotkeyKey.Tab => 0x09,
			EHotkeyKey.Space => 0x20,
			EHotkeyKey.Delete => 0x2E,
			EHotkeyKey.Home => 0x24,
			EHotkeyKey.End => 0x23,
			EHotkeyKey.PageUp => 0x21,
			EHotkeyKey.PageDown => 0x22,
			EHotkeyKey.Up => 0x26,
			EHotkeyKey.Down => 0x28,
			EHotkeyKey.Left => 0x25,
			EHotkeyKey.Right => 0x27,

			// 其他键
			EHotkeyKey.Print => 0x2C,
			EHotkeyKey.Pause => 0x13,
			EHotkeyKey.Insert => 0x2D,
			EHotkeyKey.NumLock => 0x90,
			EHotkeyKey.CapsLock => 0x14,
			EHotkeyKey.ScrollLock => 0x91,

			_ => 0
		};
	}
	// 此實現使用專用消息線程接收 WM_HOTKEY，不依賴 Avalonia 窗口消息鉤子。
}
