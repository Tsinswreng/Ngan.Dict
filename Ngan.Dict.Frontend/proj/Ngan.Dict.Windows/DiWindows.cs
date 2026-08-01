using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Core.Frontend.Hotkey;
using Ngan.Dict.Core.Shared.Audio;
using Ngan.Dict.Windows.Domains.Audio;
using Ngan.Dict.Windows.Domains.Hotkey;

namespace Ngan.Dict.Windows;

public static class DiWindows{
	public static IServiceCollection SetupWindows(this IServiceCollection Z){
		Z.AddSingleton<IAudioPlayer, NAudioPlayer>();
		Z.AddSingleton<IHotkeyListener, WinHotkeyListener>();
		Z.AddSingleton<I_RegisterGlobalHotKeys, WinGlobalHotkeyRegistrar>();
		return Z;
	}
}
