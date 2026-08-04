namespace Ngan.Dict.Android;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngan.Dict.Core.Android.Audio;
using Ngan.Dict.Core.Infra.Log;
using Ngan.Dict.Core.Shared.Audio;

public static class DiAndroid{
	public static IServiceCollection SetupAndroid(this IServiceCollection z){
/*
adb logcat -c # 清除日誌
# 显示该 Tag 的所有级别日志（V/D/I/W/E/F），并保留时间戳格式
adb logcat Tsinswreng.Ngan.Dict:V *:S -v time
 */
		z.AddSingleton<ILogger>(_=>AppLog.Inst);
		z.AddSingleton<IAudioPlayer, AndroidAudioPlayer>();
		return z;
	}
}
