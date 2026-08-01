namespace Ngaq.Android;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngaq.Android.Domains.Clipboard;
using Ngaq.Core.Android.Audio;
using Ngaq.Core.Frontend.Clipboard;
using Ngaq.Core.Infra.Log;
using Ngaq.Core.Shared.Audio;

public static class DiAndroid{
	public static IServiceCollection SetupAndroid(this IServiceCollection z){
/*
adb logcat -c # 清除日誌
# 显示该 Tag 的所有级别日志（V/D/I/W/E/F），并保留时间戳格式
adb logcat Tsinswreng.Ngaq:V *:S -v time
 */
		z.AddSingleton<ILogger>(_=>AppLog.Inst);
		z.AddSingleton<IAudioPlayer, AndroidAudioPlayer>();
		// 覆蓋 UI 層默認剪貼板服務，改用 Android 原生 ClipboardManager。
		z.AddSingleton<ISvcClipboard, AndroidSvcClipboard>();
		return z;
	}
}
