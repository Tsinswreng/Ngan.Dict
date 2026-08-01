namespace Ngaq.Windows.Domains.Hotkey;

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Ngaq.Core.Frontend.Hotkey;
using Ngaq.Ui.Views.Dictionary;
using Tsinswreng.CsErr;

/// Windows 专用的全局快捷键注册器
/// TODO 把查詞快捷键之註冊邏輯放到新文件中、然後在此文件中調用
public class WinGlobalHotkeyRegistrar : I_RegisterGlobalHotKeys{
	private const str DictionaryLookupHotkeyId = "dictionary_lookup_from_clipboard";

	private readonly IHotkeyListener _hotkeyListener;
	private readonly IHotkeyDictionaryLookupAction _dictionaryLookupAction;
	private readonly IParseDictionaryLookupHotkeyCfg _hotkeyCfgParser;
	private readonly ILogger _logger;

	public WinGlobalHotkeyRegistrar(
		IHotkeyListener hotkeyListener,
		IHotkeyDictionaryLookupAction dictionaryLookupAction,
		IParseDictionaryLookupHotkeyCfg hotkeyCfgParser,
		ILogger logger
	){
		_hotkeyListener = hotkeyListener;
		_dictionaryLookupAction = dictionaryLookupAction;
		_hotkeyCfgParser = hotkeyCfgParser;
		_logger = logger;
	}

	//TODO 叶 ˋ有蠹、AOT編譯後運行即卡住不動、頁面不出
	public IAnswer<obj?> RegisterGlobalHotKeys(){
		var R = new Answer<obj?>();

		try{
			// 從配置讀取查詞熱鍵，避免在代碼中寫死。
			var hotkeyCfg = _hotkeyCfgParser.ReadFromAppCfg(msg => _logger?.LogWarning("{WarningMessage}", msg));
			var hotkey = new HotKey{
				Id = DictionaryLookupHotkeyId,
				Modifiers = hotkeyCfg.Modifiers,
				Key = hotkeyCfg.Key,
				OnHotkey = (Req, Ct) => {
					Dispatcher.UIThread.Post(async () => {
						try{
							ShowMainWindow();
							await _dictionaryLookupAction.Run(Ct);
						}catch(Exception ex){
							_logger?.LogError(ex, "Dictionary lookup hotkey handler failed");
						}
					});
					return Task.FromResult<IRespHotKey?>(null);
				}
			};

			var result = _hotkeyListener.Register(hotkey);
			if(!result.Ok){
				_logger?.LogWarning(
					"Dictionary lookup hotkey registration failed ({Modifiers}+{Key})\n{Errors}",
					hotkeyCfg.Modifiers,
					hotkeyCfg.Key,
					string.Join(";", result.Errors ?? new[]{""})
				);
			}else{
				_logger?.LogInformation(
					"Dictionary lookup hotkey registered successfully: {Modifiers}+{Key}",
					hotkeyCfg.Modifiers,
					hotkeyCfg.Key
				);
			}

			_logger?.LogInformation("Windows global hotkeys registered successfully");
			return R.OkWith(NIL);
		}catch(Exception ex){
			return R.AddErr(ex);
		}
	}

	private static nil ShowMainWindow(){
		var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
		var mainWindow = desktop?.MainWindow;
		if(mainWindow == null){
			return NIL;
		}

		if(!mainWindow.IsVisible){
			mainWindow.Show();
		}
		if(mainWindow.WindowState == WindowState.Minimized){
			mainWindow.WindowState = WindowState.Normal;
		}
		mainWindow.Activate();
		return NIL;
	}

}
