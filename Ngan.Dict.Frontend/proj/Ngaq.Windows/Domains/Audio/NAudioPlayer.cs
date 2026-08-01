using Ngaq.Core.Shared.Audio;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using Tsinswreng.CsErr;

namespace Ngaq.Windows.Domains.Audio;

/// Windows 平台音频播放器。
/// `Wav` 仍走 NAudio；`Mp3` 改走 MCI，避开 NativeAOT 下 `Mp3FileReader` 的 marshalling 问题。
public partial class NAudioPlayer : IAudioPlayer {
	/// 播放音频流。
	/// <param name="s">音频数据流。</param>
	/// <param name="type">音频类型。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>当前暂无可控播放状态，返回 null。</returns>
	public async Task<IPlayState?> Play(Stream s, EAudioType type, CT Ct) {
		try{
			switch(type){
				case EAudioType.Wav:
					await PlayWav(s, Ct);
					break;
				case EAudioType.Mp3:
					await PlayMp3(s, Ct);
					break;
				default:
					throw ToolAudioErr.MkAudioPlayFailedErr(
						null,
						$"UnsupportedAudioType={type}",
						"Platform=Windows"
					);
			}
			return null;
		}catch(OperationCanceledException) when(Ct.IsCancellationRequested){
			throw;
		}catch(Exception ex) when(ex is not Tsinswreng.CsErr.AppErr){
			throw ToolAudioErr.MkAudioPlayFailedErr(ex, $"Type={type}", "Platform=Windows");
		}
	}

	/// `Wav` 沿用 NAudio 本地读取路径；此分支不涉及 `Mp3WaveFormat` 的 AOT marshalling。
	private static async Task<nil> PlayWav(Stream S, CT Ct){
		await PlayByWaveOut(() => new WaveFileReader(S), Ct);
		return NIL;
	}

	/// 用统一的 `WaveOutEvent` 逻辑播放已解码的 `WaveStream`。
	private static async Task<nil> PlayByWaveOut(Func<WaveStream> ReaderFactory, CT Ct){
		// 在后台线程播放，避免阻塞调用方。
		await Task.Run(() => {
			using WaveStream Reader = ReaderFactory();
			using var Wo = new WaveOutEvent();
			Wo.Init(Reader);
			Wo.Play();
			while(Wo.PlaybackState == PlaybackState.Playing){
				Ct.ThrowIfCancellationRequested();
				Thread.Sleep(100);
			}
		}, Ct);
		return NIL;
	}

	/// `Mp3` 改为走 Windows 自带 MCI，避免 NAudio 的 ACM/interop 解码路径。
	private static async Task<nil> PlayMp3(Stream S, CT Ct){
		var tempFilePath = await SaveToTempFile(S, ".mp3", Ct);
		try{
			if(await TryPlayMp3ByMediaFoundation(tempFilePath, Ct)){
				return NIL;
			}
			await Task.Run(() => PlayMp3ByMci(tempFilePath, Ct), Ct);
		}finally{
			TryDeleteTempFile(tempFilePath);
		}
		return NIL;
	}

	/// 优先尝试 Media Foundation。
	/// 这条路径仍在 NAudio 内，但不再走 `Mp3FileReader` 的 ACM marshalling 分支。
	private static async Task<bool> TryPlayMp3ByMediaFoundation(str TempFilePath, CT Ct){
		try{
			await PlayByWaveOut(() => new MediaFoundationReader(TempFilePath), Ct);
			return true;
		}catch(Exception Ex) when(Ex is not OperationCanceledException && Ex is not Tsinswreng.CsErr.AppErr){
			return false;
		}
	}

	/// 将流写入临时文件，供 MCI 直接打开播放。
	private static async Task<str> SaveToTempFile(Stream S, str Extension, CT Ct){
		var tempFilePath = Path.Combine(Path.GetTempPath(), $"ngaq-audio-{Guid.NewGuid():N}{Extension}");
		await using var Fs = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 81920, useAsync: true);
		await S.CopyToAsync(Fs, Ct);
		await Fs.FlushAsync(Ct);
		return tempFilePath;
	}

	/// 使用 MCI 打开并轮询播放状态。
	/// 这样既能兼容 AOT，也能在取消时主动 `stop/close`。
	private static nil PlayMp3ByMci(str TempFilePath, CT Ct){
		var alias = $"ngaqAudio{Guid.NewGuid():N}";
		try{
			OpenMp3ByMci(TempFilePath, alias);
			SendMciCommand($"play {alias}");

			while(true){
				Ct.ThrowIfCancellationRequested();
				var mode = GetMciMode(alias);
				if(mode is "stopped"){
					break;
				}
				Thread.Sleep(100);
			}
		}catch(OperationCanceledException){
			TrySendMciCommand($"stop {alias}");
			throw;
		}finally{
			TrySendMciCommand($"close {alias}");
		}
		return NIL;
	}

	/// 不同 Windows 环境对 `open` 命令格式兼容性不完全一致，这里按顺序回退。
	private static nil OpenMp3ByMci(str TempFilePath, str Alias){
		var openCommands = new[]{
			$"open \"{TempFilePath}\" alias {Alias}",
			$"open \"{TempFilePath}\" type mpegvideo alias {Alias}",
			$"open \"{TempFilePath}\" type MPEGVideo alias {Alias}"
		};

		AppErr? lastErr = null;
		foreach(var command in openCommands){
			try{
				SendMciCommand(command);
				return NIL;
			}catch(AppErr Ex){
				lastErr = Ex;
			}
		}

		if(lastErr is not null){
			throw lastErr;
		}
		throw ToolAudioErr.MkAudioPlayFailedErr(null, "MciOpenFailedWithoutException", "Platform=Windows");
	}

	/// 读取当前 MCI 设备状态。
	private static str GetMciMode(str Alias){
		var buffer = new char[128];
		SendMciCommand($"status {Alias} mode", buffer);
		return CharBufferToString(buffer).Trim().ToLowerInvariant();
	}

	/// 发送 MCI 命令；失败时抛统一异常，方便上层收敛日志。
	private static nil SendMciCommand(str Command, char[]? Buffer = null){
		var errCode = NativeMethods.MciSendString(Command, Buffer, Buffer?.Length ?? 0, IntPtr.Zero);
		if(errCode != 0){
			var errText = GetMciErrorText(errCode);
			throw ToolAudioErr.MkAudioPlayFailedErr(
				null,
				$"MciCommand={Command}",
				$"MciErrorCode={errCode}",
				$"MciErrorText={errText}",
				"Platform=Windows"
			);
		}
		return NIL;
	}

	/// 清理阶段尽量吞掉 MCI 错误，避免覆盖原始异常。
	private static nil TrySendMciCommand(str Command){
		try{
			SendMciCommand(Command);
		}catch{
			// 清理失败不阻断主流程。
		}
		return NIL;
	}

	/// 将 MCI 错误码转换为可读文本，便于日志定位。
	private static str GetMciErrorText(i32 ErrorCode){
		var buffer = new char[256];
		if(NativeMethods.MciGetErrorString(ErrorCode, buffer, buffer.Length)){
			return CharBufferToString(buffer).Trim();
		}
		return "UnknownMciError";
	}

	/// MCI 返回的是以 `\0` 结尾的字符缓冲区，这里只截取有效内容。
	private static str CharBufferToString(char[] Buffer){
		var end = Array.IndexOf(Buffer, '\0');
		if(end < 0){
			end = Buffer.Length;
		}
		return new string(Buffer, 0, end);
	}

	/// 删除临时文件；失败时忽略，避免影响主流程。
	private static nil TryDeleteTempFile(str TempFilePath){
		try{
			if(File.Exists(TempFilePath)){
				File.Delete(TempFilePath);
			}
		}catch{
			// 临时文件清理失败不阻断业务流程。
		}
		return NIL;
	}

	/// AOT 友好的 WinMM P/Invoke。
	private static partial class NativeMethods{
		[LibraryImport("winmm.dll", EntryPoint = "mciSendStringW", StringMarshalling = StringMarshalling.Utf16)]
		internal static partial int MciSendString(string Command, [Out] char[]? ReturnString, int ReturnLength, IntPtr Callback);

		[LibraryImport("winmm.dll", EntryPoint = "mciGetErrorStringW", StringMarshalling = StringMarshalling.Utf16)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static partial bool MciGetErrorString(int ErrorCode, [Out] char[] ErrorText, int ErrorTextSize);
	}
}
