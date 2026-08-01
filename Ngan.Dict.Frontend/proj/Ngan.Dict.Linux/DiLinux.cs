using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Core.Frontend.Hotkey;
using Ngan.Dict.Core.Shared.Audio;
using Ngan.Dict.Linux.Domains.Audio;
using Ngan.Dict.Linux.Domains.Hotkey;

namespace Ngan.Dict.Linux;

public static class DiLinux{
	public static IServiceCollection SetupLinux(this IServiceCollection Z){
		Z.AddSingleton<IHotkeyListener, LinuxHotkeyListener>();
		Z.AddSingleton<I_RegisterGlobalHotKeys, LinuxGlobalHotkeyRegistrar>();
		// Linux 端使用 LinuxAudioPlayer，避免依賴 Windows 專用庫。
		Z.AddSingleton<IAudioPlayer, LinuxAudioPlayer>();
		return Z;
	}
}
