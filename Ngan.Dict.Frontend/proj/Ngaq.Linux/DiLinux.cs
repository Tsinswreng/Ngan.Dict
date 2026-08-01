using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core.Frontend.Hotkey;
using Ngaq.Core.Shared.Audio;
using Ngaq.Linux.Domains.Audio;
using Ngaq.Linux.Domains.Hotkey;

namespace Ngaq.Linux;

public static class DiLinux{
	public static IServiceCollection SetupLinux(this IServiceCollection Z){
		Z.AddSingleton<IHotkeyListener, LinuxHotkeyListener>();
		Z.AddSingleton<I_RegisterGlobalHotKeys, LinuxGlobalHotkeyRegistrar>();
		// Linux 端使用 LinuxAudioPlayer，避免依賴 Windows 專用庫。
		Z.AddSingleton<IAudioPlayer, LinuxAudioPlayer>();
		return Z;
	}
}
