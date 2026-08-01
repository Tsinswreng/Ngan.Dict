using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core.Frontend.Hotkey;
using Ngaq.Core.Shared.Audio;
using Ngaq.Windows.Domains.Audio;
using Ngaq.Windows.Domains.Hotkey;

namespace Ngaq.Windows;

public static class DiWindows{
	public static IServiceCollection SetupWindows(this IServiceCollection Z){
		Z.AddSingleton<IAudioPlayer, NAudioPlayer>();
		Z.AddSingleton<IHotkeyListener, WinHotkeyListener>();
		Z.AddSingleton<I_RegisterGlobalHotKeys, WinGlobalHotkeyRegistrar>();
		return Z;
	}
}
