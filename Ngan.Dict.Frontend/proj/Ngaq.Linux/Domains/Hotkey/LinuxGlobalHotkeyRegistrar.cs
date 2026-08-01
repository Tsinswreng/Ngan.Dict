namespace Ngaq.Linux.Domains.Hotkey;

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Ngaq.Core.Frontend.Hotkey;
using Ngaq.Ui.Views.Dictionary;
using Tsinswreng.CsErr;

/// Linux 專用全局熱鍵註冊器。
/// 職責只包含: 讀配置、構造 HotKey、委託給 IHotkeyListener。
public class LinuxGlobalHotkeyRegistrar : I_RegisterGlobalHotKeys{
	private const str DictionaryLookupHotkeyId = "dictionary_lookup_from_clipboard";

	private readonly IHotkeyListener _hotkeyListener;
	private readonly IHotkeyDictionaryLookupAction _dictionaryLookupAction;
	private readonly IParseDictionaryLookupHotkeyCfg _hotkeyCfgParser;
	private readonly ILogger _logger;

	public LinuxGlobalHotkeyRegistrar(
		IHotkeyListener HotkeyListener,
		IHotkeyDictionaryLookupAction DictionaryLookupAction,
		IParseDictionaryLookupHotkeyCfg HotkeyCfgParser,
		ILogger Logger
	){
		_hotkeyListener = HotkeyListener;
		_dictionaryLookupAction = DictionaryLookupAction;
		_hotkeyCfgParser = HotkeyCfgParser;
		_logger = Logger;
	}

	/// 註冊 Linux 全局快捷鍵。
	/// <returns>註冊成功返回 Ok，失敗時在 Answer 中返回錯誤。</returns>
	public IAnswer<obj?> RegisterGlobalHotKeys(){
		var Answer = new Answer<obj?>();
		try{
			// 從配置讀取查詞熱鍵，保持與 Windows 路徑一致。
			var HotkeyCfg = _hotkeyCfgParser.ReadFromAppCfg(Msg => _logger?.LogWarning("{WarningMessage}", Msg));
			var Hotkey = new HotKey{
				Id = DictionaryLookupHotkeyId,
				Modifiers = HotkeyCfg.Modifiers,
				Key = HotkeyCfg.Key,
				OnHotkey = (Req, Ct) => {
					Dispatcher.UIThread.Post(async () => {
						try{
							ShowMainWindow();
							await _dictionaryLookupAction.Run(Ct);
						}catch(Exception Ex){
							_logger?.LogError(Ex, "Linux dictionary lookup hotkey handler failed");
						}
					});
					return Task.FromResult<IRespHotKey?>(null);
				}
			};

			var Result = _hotkeyListener.Register(Hotkey);
			if(!Result.Ok){
				_logger?.LogWarning(
					"Linux dictionary lookup hotkey registration failed ({Modifiers}+{Key})\n{Errors}",
					HotkeyCfg.Modifiers,
					HotkeyCfg.Key,
					string.Join(";", Result.Errors ?? new[]{""})
				);
				return Answer.AddErr(
					$"Linux hotkey registration failed: {HotkeyCfg.Modifiers}+{HotkeyCfg.Key}"
				);
			}else{
				_logger?.LogInformation(
					"Linux dictionary lookup hotkey registered: {Modifiers}+{Key}",
					HotkeyCfg.Modifiers,
					HotkeyCfg.Key
				);
			}

			return Answer.OkWith(NIL);
		}catch(Exception Ex){
			return Answer.AddErr(Ex);
		}
	}

	/// 嘗試喚起主窗口，方便快捷鍵觸發後立即可見。
	private static nil ShowMainWindow(){
		var Desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
		var MainWindow = Desktop?.MainWindow;
		if(MainWindow == null){
			return NIL;
		}

		if(!MainWindow.IsVisible){
			MainWindow.Show();
		}
		if(MainWindow.WindowState == WindowState.Minimized){
			MainWindow.WindowState = WindowState.Normal;
		}
		MainWindow.Activate();
		return NIL;
	}
}
