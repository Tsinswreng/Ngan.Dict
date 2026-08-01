namespace Ngaq.Ui;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Ngaq.Core.Frontend.Clipboard;

/// 基於 Avalonia 的跨平臺剪貼板服務。
public class SvcClipboard : ISvcClipboard{
	/// 讀取剪貼板文本。
	/// <param name="Ct">取消令牌。</param>
	/// <returns>剪貼板文本；若不可用則返回 null。</returns>
	public async Task<str?> GetText(CT Ct){
		Ct.ThrowIfCancellationRequested();

		var clipboard = ResolveClipboard();
		if(clipboard is null){
			return null;
		}
		return await clipboard.TryGetTextAsync();
	}

	/// 從桌面主窗口對應的 TopLevel 拿到 Avalonia Clipboard。
	/// <returns>剪貼板實例；若當前無窗口則返回 null。</returns>
	private static Avalonia.Input.Platform.IClipboard? ResolveClipboard(){
		var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
		var mainWindow = desktop?.MainWindow;
		if(mainWindow is null){
			return null;
		}
		var topLevel = TopLevel.GetTopLevel(mainWindow);
		return topLevel?.Clipboard ?? mainWindow?.Clipboard;
	}
}
