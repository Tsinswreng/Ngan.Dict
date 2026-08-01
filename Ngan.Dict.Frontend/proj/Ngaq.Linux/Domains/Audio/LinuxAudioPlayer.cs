namespace Ngaq.Linux.Domains.Audio;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ngaq.Core.Shared.Audio;

/// Linux 平臺音頻播放器。
/// 使用外部命令行播放器進行回放，避免依賴 Windows 專用音頻庫。
public class LinuxAudioPlayer : IAudioPlayer{
	private sealed record PlayerCmd(str FileName, str ArgsTemplate);
	private sealed record PlayerTryResult(bool Ok, str Reason);
	private sealed record ResolvedPlayerCmd(str OriginalName, str ExecutablePath, str ArgsTemplate);

	// Ubuntu 22 常見播放器命令回退鏈。
	private static readonly IReadOnlyList<PlayerCmd> Mp3Players = [
		new("mpg123", "-q \"{0}\""),
		new("ffplay", "-nodisp -autoexit -loglevel quiet \"{0}\""),
		new("cvlc", "--play-and-exit --quiet \"{0}\""),
		new("mpv", "--no-video --really-quiet \"{0}\""),
		new("gst-play-1.0", "-q \"{0}\""),
		new("play", "-q \"{0}\"")
	];

	private static readonly IReadOnlyList<PlayerCmd> WavPlayers = [
		new("aplay", "\"{0}\""),
		new("paplay", "\"{0}\""),
		new("ffplay", "-nodisp -autoexit -loglevel quiet \"{0}\""),
		new("cvlc", "--play-and-exit --quiet \"{0}\""),
		new("mpv", "--no-video --really-quiet \"{0}\""),
		new("gst-play-1.0", "-q \"{0}\""),
		new("play", "-q \"{0}\"")
	];

	/// 播放音頻流。
	/// <param name="S">音頻數據流。</param>
	/// <param name="Type">音頻類型（Mp3/Wav）。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>暫無可控播放狀態，返回 null。</returns>
	public async Task<IPlayState?> Play(Stream S, EAudioType Type, CT Ct){
		try{
			var extension = GetAudioExtension(Type);
			var tempFilePath = await SaveToTempFile(S, extension, Ct);

			try{
				await Task.Run(() => PlayWithFallbackCommands(tempFilePath, Type, Ct), Ct);
				return null;
			}finally{
				TryDeleteTempFile(tempFilePath);
			}
		}catch(OperationCanceledException) when(Ct.IsCancellationRequested){
			throw;
		}catch(Exception ex) when(ex is not Tsinswreng.CsErr.AppErr){
			throw ToolAudioErr.MkAudioPlayFailedErr(ex, $"Type={Type}", "Platform=Linux");
		}
	}

	/// 根據音頻類型選擇臨時文件副檔名。
	private static str GetAudioExtension(EAudioType Type){
		return Type switch{
			EAudioType.Mp3 => ".mp3",
			EAudioType.Wav => ".wav",
			_ => throw ToolAudioErr.MkAudioPlayFailedErr(
				null,
				$"UnsupportedAudioType={Type}",
				"Platform=Linux"
			)
		};
	}

	/// 將流寫入臨時文件，供外部播放器直接讀取。
	private static async Task<str> SaveToTempFile(Stream S, str Extension, CT Ct){
		var tempFilePath = Path.Combine(Path.GetTempPath(), $"ngaq-audio-{Guid.NewGuid():N}{Extension}");
		await using var fs = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 81920, useAsync: true);
		await S.CopyToAsync(fs, Ct);
		await fs.FlushAsync(Ct);
		return tempFilePath;
	}

	/// 依序嘗試可用播放器，直到成功播放或全部失敗。
	private static nil PlayWithFallbackCommands(str TempFilePath, EAudioType Type, CT Ct){
		var players = Type == EAudioType.Mp3 ? Mp3Players : WavPlayers;
		var reasons = new List<str>();

		foreach(var player in players){
			var result = TryPlayByCommand(player, TempFilePath, Ct);
			if(result.Ok){
				return NIL;
			}
			reasons.Add($"{player.FileName}:{result.Reason}");
		}

		throw ToolAudioErr.MkAudioPlayFailedErr(
			null,
			$"NoAvailableLinuxPlayerFor={Type}",
			$"Tried={string.Join(",", players)}",
			$"Reasons={string.Join("|", reasons)}",
			"InstallHint=sudo apt-get update && sudo apt-get install -y mpv ffmpeg vlc mpg123 gstreamer1.0-tools sox alsa-utils pulseaudio-utils",
			"Platform=Linux"
		);
	}

	/// 使用指定命令播放文件；命令不存在或退出失敗時返回 false。
	private static PlayerTryResult TryPlayByCommand(PlayerCmd Player, str TempFilePath, CT Ct){
		try{
			var resolved = ResolvePlayerCommand(Player);
			if(resolved is null){
				return new PlayerTryResult(false, $"ExecutableNotFound:{Player.FileName}");
			}

			var sw = Stopwatch.StartNew();
			var psi = new ProcessStartInfo{
				FileName = resolved.ExecutablePath,
				Arguments = string.Format(resolved.ArgsTemplate, TempFilePath),
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};

			using var proc = Process.Start(psi);
			if(proc == null){
				return new PlayerTryResult(false, "ProcessStartReturnedNull");
			}

			using var reg = Ct.Register(() => {
				try{
					if(!proc.HasExited){
						proc.Kill(entireProcessTree: true);
					}
				}catch{
					// 取消過程的清理錯誤不影響主流程。
				}
			});

			proc.WaitForExit();
			Ct.ThrowIfCancellationRequested();
			sw.Stop();
			if(proc.ExitCode == 0){
				if(sw.ElapsedMilliseconds < 200){
					return new PlayerTryResult(false, $"ExitedTooFastMs={sw.ElapsedMilliseconds}");
				}
				return new PlayerTryResult(true, $"ExitCode=0,Executable={resolved.ExecutablePath}");
			}
			return new PlayerTryResult(false, $"ExitCode={proc.ExitCode}");
		}catch(Exception ex){
			if(ex is System.ComponentModel.Win32Exception w32){
				return new PlayerTryResult(
					false,
					$"{ex.GetType().Name}(NativeErrorCode={w32.NativeErrorCode},Message={w32.Message})"
				);
			}
			return new PlayerTryResult(false, $"{ex.GetType().Name}({ex.Message})");
		}
	}

	/// 解析播放器可執行文件的絕對路徑。
	private static ResolvedPlayerCmd? ResolvePlayerCommand(PlayerCmd Player){
		if(Path.IsPathRooted(Player.FileName) && File.Exists(Player.FileName)){
			return new ResolvedPlayerCmd(Player.FileName, Player.FileName, Player.ArgsTemplate);
		}

		var candidates = new List<str>();
		var pathValue = Environment.GetEnvironmentVariable("PATH") ?? "";
		var pathDirs = pathValue
			.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		candidates.AddRange(pathDirs.Select(dir => Path.Combine(dir, Player.FileName)));

		candidates.Add($"/usr/bin/{Player.FileName}");
		candidates.Add($"/bin/{Player.FileName}");
		candidates.Add($"/usr/local/bin/{Player.FileName}");
		candidates.Add($"/snap/bin/{Player.FileName}");

		foreach(var candidate in candidates.Distinct(StringComparer.Ordinal)){
			try{
				if(File.Exists(candidate)){
					return new ResolvedPlayerCmd(Player.FileName, candidate, Player.ArgsTemplate);
				}
			}catch{
				// 路徑檢查失敗時跳過，繼續下一候選。
			}
		}

		return null;
	}

	/// 清理臨時文件，避免磁盤遺留。
	private static nil TryDeleteTempFile(str TempFilePath){
		try{
			if(File.Exists(TempFilePath)){
				File.Delete(TempFilePath);
			}
		}catch{
			// 臨時文件清理失敗不阻斷業務流程。
		}
		return NIL;
	}
}
